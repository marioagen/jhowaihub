Cypress.on('uncaught:exception', (err, runnable) => {
  return false;
});

describe('Fluxo de Upload de Documento PDF', () => {
  beforeEach(() => {
    // Acessa a página de login ou upload
    cy.visit('/')

    // Preenche usuário
    cy.get('input[name="email"]').type('lfmarques@latam.stefanini.com')

    // Preenche senha
    cy.get('input[name="password"]').type('Lfm06233104@', { log: false }) // { log: false } evita expor senha no log

    // Clica no botão de login usando o texto do botão
    cy.get('button[type="button"]').click()

    // Valida que o login foi bem-sucedido
    cy.url().should('include', '/documents')
    cy.contains('Documentos').should('be.visible')
  })

it('Deve realizar o upload de um arquivo PDF com sucesso', () => {
    // Clica no botão para iniciar um novo documento
    cy.intercept('GET', 'https://hub-api.qa.woopi.ai/api/Team').as('apiTeam');
    //GET 200 https://hub-api.qa.woopi.ai/api/Team
    cy.intercept('POST', 'https://hub-api.qa.woopi.ai/api/Document/UploadByChunks').as('UploadByChunks');
    //POST 200 https://hub-api.qa.woopi.ai/api/Document/UploadByChunks
    cy.contains('Novo documento', { timeout: 7000 }).should('be.visible').click();
    cy.url().should('include', '/documents/upload');
    cy.wait('@apiTeam').its('response.statusCode').should('eq', 200);
    
    // Caminho do arquivo PDF que ficará dentro de cypress/fixtures
    const filePath = 'Teste_Cypress.pdf';

    // Seleciona o input de upload e envia o arquivo PDF
    cy.get('input[type="file"]').attachFile(filePath);

    // Valida se o arquivo foi reconhecido pelo sistema
    cy.contains('Teste_Cypress.pdf').should('be.visible');

    // Digita a descrição
    cy.get('textarea[name="text"]').type('Documento de teste automatizado');

    // Seleciona um ou mais times
    cy.contains('Selecionar Todos').click(); 

    // Envia o formulário
    cy.contains('Enviar').click();
    cy.url().should('include', '/workflow')

    // Valida mensagem de sucesso
    cy.wait('@UploadByChunks').its('response.statusCode').should('eq', 200);
    cy.get('.content-wrapper > .position-fixed').contains('Todos os arquivos carregados');
  });
});