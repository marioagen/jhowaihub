describe('Login no sistema', () => {
  beforeEach(() => {
    // Acessa a página de login
    cy.visit('/')
  })

  it('Deve logar com credenciais válidas', () => {
    // Preenche usuário
    cy.get('input[name="email"]').type('lfmarques@latam.stefanini.com')

    // Preenche senha
    cy.get('input[name="password"]').type('Lfm06233104@', { log: false }) // { log: false } evita expor senha no log

    // Clica no botão de login
    cy.get('button[type="button"]').click()

    // Valida que o login foi bem-sucedido
    cy.url().should('include', '/documents')
    cy.contains('Documentos').should('be.visible')
  })
  })