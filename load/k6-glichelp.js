// ============================================================
// Teste de carga GlicHelp — usuários REAIS e diferentes.
//
// Pré-requisitos:
//   1. database/02-seed-carga.sql executado (cria carga1..carga10000@teste.com)
//   2. API rodando no ambiente LoadTest (rate limiting alto):
//        dotnet run -c Release --project Presentation --launch-profile http --environment LoadTest
//
// Rodar:
//   k6 run load/k6-glichelp.js
//       10.000 usuários simultâneos, cada um fazendo uma ação a cada 5–15 s
//       (~1.000 requisições/s — já muito acima do uso real de um app de glicemia)
//   k6 run -e PAUSA_MIN=1 -e PAUSA_MAX=3 load/k6-glichelp.js
//       cenário extremo (~5.000 req/s): mede o limite da máquina
//   k6 run -e MAX_VUS=2000 load/k6-glichelp.js      (teste menor)
//
// Cada usuário virtual (VU) faz login UMA vez com a própria conta e depois
// se comporta como uma pessoa usando o app: vê o histórico, o perfil e
// de vez em quando cadastra uma glicemia, com uma pausa entre as ações.
// ============================================================

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5288/api';
const MAX_VUS = Number(__ENV.MAX_VUS || 10000);
const PAUSA_MIN = Number(__ENV.PAUSA_MIN || 5);   // segundos entre ações (mínimo)
const PAUSA_MAX = Number(__ENV.PAUSA_MAX || 15);  // segundos entre ações (máximo)

const erros5xx = new Counter('erros_5xx');

export const options = {
    // Rampa mais longa: os 10.000 logins (BCrypt é pesado de propósito) ficam espalhados.
    stages: [
        { duration: '2m', target: Math.round(MAX_VUS * 0.2) },
        { duration: '3m', target: MAX_VUS },
        { duration: '3m', target: MAX_VUS },   // platô: carga máxima sustentada
        { duration: '1m', target: 0 },
    ],
    thresholds: {
        'http_req_failed': ['rate<0.01'],                          // menos de 1% de falhas
        'http_req_duration{tipo:leitura}': ['p(95)<300'],          // 95% das leituras < 300 ms
        'http_req_duration{tipo:escrita}': ['p(95)<500'],          // 95% das escritas < 500 ms
        'erros_5xx': ['count==0'],
    },
};

const JSON_HEADERS = { 'Content-Type': 'application/json' };
const REFEICOES = ['Café da Manhã', 'Almoço', 'Lanche', 'Jantar'];

// Token guardado por VU (cada VU é um usuário diferente)
let token = null;

function login() {
    const email = `carga${__VU}@teste.com`;

    const res = http.post(
        `${BASE_URL}/usuario/login`,
        JSON.stringify({ email, senha: 'Carga@123' }),
        { headers: JSON_HEADERS, tags: { tipo: 'login' } }
    );

    check(res, { 'login 200': (r) => r.status === 200 });
    contar5xx(res);

    return res.status === 200 ? res.json('token') : null;
}

function contar5xx(res) {
    if (res.status >= 500) {
        erros5xx.add(1);
    }
}

export default function () {
    if (!token) {
        token = login();
        if (!token) {
            sleep(PAUSA_MIN);
            return;
        }
    }

    const auth = { headers: { ...JSON_HEADERS, Authorization: `Bearer ${token}` } };
    const sorteio = Math.random();

    if (sorteio < 0.6) {
        // Histórico / dashboard: últimos 50 registros
        const res = http.get(`${BASE_URL}/registro-glicemia?pagina=1&tamanho=50`,
            { ...auth, tags: { tipo: 'leitura', rota: 'historico' } });
        check(res, { 'histórico 200': (r) => r.status === 200 });
        contar5xx(res);
    } else if (sorteio < 0.8) {
        // Perfil (calculadora usa HGT alvo e fator de sensibilidade)
        const res = http.get(`${BASE_URL}/usuario/perfil`,
            { ...auth, tags: { tipo: 'leitura', rota: 'perfil' } });
        check(res, { 'perfil 200': (r) => r.status === 200 });
        contar5xx(res);
    } else {
        // Novo registro de glicemia
        const agora = new Date();
        const payload = {
            glicemia: 80 + Math.floor(Math.random() * 150),
            glicemiaAcimaDoLimite: false,
            dose: Math.floor(Math.random() * 20) / 2,
            hora: agora.toTimeString().slice(0, 8),
            refeicao: REFEICOES[Math.floor(Math.random() * REFEICOES.length)],
            data: agora.toISOString().slice(0, 10),
            observacao: null,
        };

        const res = http.post(`${BASE_URL}/registro-glicemia`, JSON.stringify(payload),
            { ...auth, tags: { tipo: 'escrita', rota: 'criar-registro' } });
        check(res, { 'criar 201': (r) => r.status === 201 });
        contar5xx(res);
    }

    sleep(PAUSA_MIN + Math.random() * (PAUSA_MAX - PAUSA_MIN));
}
