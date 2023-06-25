/** @type {import('tailwindcss').Config} */
module.exports = {
  separator: "_",
  content: ["./index.html", "./src/**/*.{vue,js,ts,jsx,tsx}"],
  theme: {
    extend: {
      colors: {
        "theme-darkest": "#1b262c",
        "theme-dark": "#0f4c75",
        "theme-light": "#3282b8",
        "theme-lightest": "#bbe1fa",
      },
    },
  },
  plugins: [],
};
