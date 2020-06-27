module.exports = {
  root: true,
  env: {
    node: true
  },
  extends: [
    "plugin:vue/essential",
    "@vue/standard",
    "@vue/typescript/recommended"
  ],
  parserOptions: {
    ecmaVersion: 2020
  },
  rules: {
    "no-console": process.env.NODE_ENV === "production" ? "error" : "off",
    "no-debugger": process.env.NODE_ENV === "production" ? "error" : "off",
    "eol-last": ["error", "never"],
    quotes: ["error", "double", { avoidEscape: true }],
    semi: ["error", "always"],
    "space-before-function-paren": ["error", "never"],
    "no-extend-native": "off",
    "no-empty-function": "off",
    "@typescript-eslint/no-empty-function": "off",
    "quote-props": "off"
  },
  overrides: [
    {
      files: [
        "**/__tests__/*.{j,t}s?(x)",
        "**/tests/unit/**/*.spec.{j,t}s?(x)"
      ],
      env: {
        jest: true
      }
    }
  ]
};