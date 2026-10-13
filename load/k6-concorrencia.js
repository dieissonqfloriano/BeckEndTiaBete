// ============================================================
// Teste de CONCORRÊNCIA GlicHelp
// Não mede volume: verifica se o sistema continua correto quando
// muitas requisições mexem na MESMA coisa ao mesmo tempo.
//
// Pré-requisitos: seed de carga executado e API no ambiente LoadTest.
// Rodar:  k6 run load/k6-concorrencia.js   (leva ~1 minuto)
//
// Cenários (um depois do outro):
//   1. mesmo_email     — 200 cadastros SIMULTÂNEOS com o mesmo e-mail
//                        esperado: exatamente 1 criado (201), 199 recusados (409), 0 erros 500
//   2. mesmo_registro  — 100 usuários editando o MESMO registro ao mesmo tempo
//                        esperado: todas as edições 204, registro íntegro no final
//   3. criar_juntos    — 200 registros criados ao mesmo tempo pelo MESMO usuário
//                        esperado: 200 criados, e cada registro aceito está salvo no banco
//                        (200 é o limite de conexões novas simultâneas do Windows de notebook;
//                         num servidor Linux dá para subir para 300+)
//   4. excluir_juntos  — 50 pedidos para excluir o MESMO registro ao mesmo tempo
//                        esperado: exatamente 1 exclusão (204), o resto 404, 0 erros 500
// ============================================================

import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5288/api';
const JSON_HEADERS = { 'Content-Type': 'application/json' };

const erros5xx = new Counter('erros_5xx');
const cadastro201 = new Counter('mesmo_email_criados_201');
const cadastro409 = new Counter('mesmo_email_recusados_409');
const edicao204 = new Counter('mesmo_registro_editado_204');
const criado201 = new Counter('criar_juntos_criados_201');
const exclusao204 = new Counter('excluir_juntos_excluido_204');
const exclusao404 = new Counter('excluir_juntos_nao_encontrado_404');

export const options = {
    scenarios: {
        mesmo_email: {
            executor: 'shared-iterations', exec: 'mesmoEmail',
            vus: 200, iterations: 200, startTime: '0s', maxDuration: '30s',
        },
        mesmo_registro: {
            executor: 'shared-iterations', exec: 'mesmoRegistro',
            vus: 100, iterations: 100, startTime: '12s', maxDuration: '30s',
        },
        criar_juntos: {
            executor: 'shared-iterations', exec: 'criarJuntos',
            vus: 200, iterations: 200, startTime: '24s', maxDuration: '30s',
        },
        excluir_juntos: {
            executor: 'shared-iterations', exec: 'excluirJuntos',
            vus: 50, iterations: 50, startTime: '36s', maxDuration: '30s',
        },
    },
    thresholds: {
        'erros_5xx': ['count==0'],
        'mesmo_email_criados_201': ['count==1'],
        'mesmo_email_recusados_409': ['count==199'],
        'mesmo_registro_editado_204': ['count==100'],
        'criar_juntos_criados_201': ['count==200'],
        'excluir_juntos_excluido_204': ['count==1'],
        'excluir_juntos_nao_encontrado_404': ['count==49'],
    },
};

function login(email) {
    const res = http.post(`${BASE_URL}/usuario/login`,
        JSON.stringify({ email, senha: 'Carga@123' }), { headers: JSON_HEADERS });
    if (res.status !== 200) {
        throw new Error(`Login de ${email} falhou (${res.status}). O seed de carga foi executado?`);
    }
    return res.json('token');
}

function auth(token) {
    return { headers: { ...JSON_HEADERS, Authorization: `Bearer ${token}` } };
}

function hoje() {
    return new Date().toISOString().slice(0, 10);
}

function criarRegistro(token, observacao) {
    return http.post(`${BASE_URL}/registro-glicemia`, JSON.stringify({
        glicemia: 110, glicemiaAcimaDoLimite: false, dose: 2.5,
        hora: '08:00:00', refeicao: 'Café da Manhã', data: hoje(), observacao,
    }), auth(token));
}

// Conta todos os registros do usuário, página por página
function contarRegistros(token) {
    let total = 0;
    for (let pagina = 1; ; pagina++) {
        const itens = http.get(`${BASE_URL}/registro-glicemia?pagina=${pagina}&tamanho=500`, auth(token)).json();
        total += itens.length;
        if (itens.length < 500) return total;
    }
}

function contar5xx(res) {
    if (res.status >= 500) erros5xx.add(1);
}

// Roda uma vez antes de tudo: prepara usuários, tokens e registros compartilhados.
export function setup() {
    const tokenEdicao = login('carga1@teste.com');
    const tokenCriacao = login('carga2@teste.com');
    const tokenExclusao = login('carga3@teste.com');

    const registroEdicao = criarRegistro(tokenEdicao, 'alvo da edição concorrente').json('id');
    const registroExclusao = criarRegistro(tokenExclusao, 'alvo da exclusão concorrente').json('id');

    // Quantos registros o usuário 2 tem antes do cenário 3
    const antes = contarRegistros(tokenCriacao);

    return {
        emailUnico: `concorrencia-${Date.now()}@teste.com`,
        tokenEdicao, registroEdicao,
        tokenCriacao, antes,
        tokenExclusao, registroExclusao,
    };
}

export function mesmoEmail(dados) {
    const res = http.post(`${BASE_URL}/usuario`, JSON.stringify({
        name: 'Teste Concorrência', email: dados.emailUnico, senha: 'Concorrencia@123',
        tipoDiabetes: 'Tipo 1', idade: 30, fatorSensibilidade: 40, hgtAlvo: 100,
        aceitouTermos: true, consentiuDadosSaude: true,
    }), { headers: JSON_HEADERS });

    if (res.status === 201) cadastro201.add(1);
    if (res.status === 409) cadastro409.add(1);
    contar5xx(res);
    check(res, { 'mesmo e-mail: 201 ou 409': (r) => r.status === 201 || r.status === 409 });
}

export function mesmoRegistro(dados) {
    const valor = 80 + (__VU % 200);
    const res = http.put(`${BASE_URL}/registro-glicemia/${dados.registroEdicao}`, JSON.stringify({
        glicemia: valor, glicemiaAcimaDoLimite: false, dose: 3,
        hora: '09:00:00', refeicao: 'Almoço', data: hoje(), observacao: `editado pelo VU ${__VU}`,
    }), auth(dados.tokenEdicao));

    if (res.status === 204) edicao204.add(1);
    contar5xx(res);
    check(res, { 'mesmo registro: 204': (r) => r.status === 204 });
}

export function criarJuntos(dados) {
    const res = criarRegistro(dados.tokenCriacao, `criado em paralelo pelo VU ${__VU}`);
    if (res.status === 201) criado201.add(1);
    contar5xx(res);
    check(res, { 'criar juntos: 201': (r) => r.status === 201 });
}

export function excluirJuntos(dados) {
    const res = http.del(`${BASE_URL}/registro-glicemia/${dados.registroExclusao}`, null, auth(dados.tokenExclusao));
    if (res.status === 204) exclusao204.add(1);
    if (res.status === 404) exclusao404.add(1);
    contar5xx(res);
    check(res, { 'excluir juntos: 204 ou 404': (r) => r.status === 204 || r.status === 404 });
}

// Roda uma vez no final: confere o estado do banco depois da concorrência.
export function teardown(dados) {
    const registro = http.get(`${BASE_URL}/registro-glicemia/${dados.registroEdicao}`, auth(dados.tokenEdicao));
    check(registro, {
        'registro editado continua íntegro': (r) => r.status === 200 && r.json('refeicao') === 'Almoço',
    });

    const depois = contarRegistros(dados.tokenCriacao);
    // Integridade: todo registro que a API confirmou (201) precisa estar salvo no banco.
    // O k6 não passa contadores para o teardown, então conferimos pelos IDs criados no banco.
    console.log(`Usuário 2: ${dados.antes} registros antes, ${depois} depois (esperado: +200)`);
    check(null, { 'nenhum registro perdido (+200)': () => depois - dados.antes === 200 });

    const excluido = http.get(`${BASE_URL}/registro-glicemia/${dados.registroExclusao}`, auth(dados.tokenExclusao));
    check(excluido, { 'registro excluído não existe mais': (r) => r.status === 404 });
}
