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

it('Deve realizar a criação de um tipo', () => {
    // Muda o usuário da tela de documentos para a tela de tipos
    cy.contains('Tipos', { timeout: 7000 }).should('be.visible').click();
    cy.url().should('include', '/types');

    // Entra na tela de criação de tipos
    cy.contains('Criar tipo', { timeout: 7000 }).should('be.visible').click();

    // Preenche o nome do tipo
    cy.get('.show > .modal-dialog > .modal-content > .modal-body > .form-control').type('Cypress');

    // Clica em criar
    cy.wait(1000) // espera um segundo
    cy.get('.show > .modal-dialog > .modal-content > .modal-footer > .btn-primary').click();

    // Valida se o tipo foi criado
    cy.contains('Tipos: Tipo de documento inserido com sucesso').should('be.visible');
  });
});