const { defineConfig } = require("cypress");
const env = require("./cypress.env.json");

module.exports = defineConfig({
  e2e: {
    setupNodeEvents(on, config) {
      // implement node event listeners here
    },
    baseUrl: env.baseUrl
  },
      defaultCommandTimeout: 20000,
      requestTimeout: 20000,
      responseTimeout: 20000,
});
