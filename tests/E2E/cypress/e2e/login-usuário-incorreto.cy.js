describe('Login incorreto no sistema', () => {
  beforeEach(() => {
    // Acessa a página de login
    cy.visit('/')
  })

  it('Não deve logar com senhas inválidas', () => {
    // Preenche usuário
    cy.get('input[name="email"]').type('lfmarques@latam.stefanini.com')

    // Preenche senha incorreta
    cy.get('input[name="password"]').type('Lfm06233104', { log: false }) // { log: false } evita expor senha no log

    // Clica no botão de login
    cy.get('button[type="button"]').click()

    // Valida que o login não foi bem-sucedido
    cy.contains('Error: O Usúario informada está incorreta.').should('be.visible')
  })
  })