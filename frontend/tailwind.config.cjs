/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./index.html", "./src/**/*.{vue,js,ts,jsx,tsx}"],
  theme: {
    extend: {
      colors: {
        "polar-darkest": "#2e3440",
        "polar-dark": "#3b4252",
        "polar-light": "#434c5e",
        "polar-lightest": "#4c566a",
        "snow-dark": "#d8dee9",
        "snow-light": "#e5e9f0",
        "snow-lightest": "#eceff4",
        "frost-green": "#8fbcbb",
        "frost-cyan": "#88c0d0",
        "frost-blue": "#81a1c1",
        "frost-deep": "#5e81ac",
        "aurora-red": "#bf616a",
        "aurora-orange": "#d08770",
        "aurora-yellow": "#ebcb8b",
        "aurora-green": "#a3be8c",
        "aurora-purple": "#b48ead",
      },
    },
  },
  plugins: [],
};
