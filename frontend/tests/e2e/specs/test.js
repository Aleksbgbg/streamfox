// https://docs.cypress.io/api/introduction/api.html

describe("My First Test", () => {
  it("Visits the app root thumbnailUrl", () => {
    cy.visit("/");
    cy.contains("h1", "Welcome to Your Vue.js + TypeScript App");
  });
});