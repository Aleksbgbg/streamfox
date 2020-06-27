module.exports = {
  separator: "-",
  purge: [
    "./src/**/*.html",
    "./src/**/*.vue",
    "./src/**/*.jsx"
  ],
  theme: {
    extend: {
      colors: {
        "theme-darkest": "#1b262c",
        "theme-dark": "#0f4c75",
        "theme-light": "#3282b8",
        "theme-lightest": "#bbe1fa"
      }
    }
  },
  variants: {},
  plugins: []
};