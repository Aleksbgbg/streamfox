module.exports = {
  plugins: [require.resolve("@trivago/prettier-plugin-sort-imports")],
  importOrder: ["^vue$", "^@?vue.*$", "<THIRD_PARTY_MODULES>", "^@/.*$"],
  importOrderSeparation: false,
  importOrderSortSpecifiers: true,
  printWidth: 100,
  tabWidth: 2,
  bracketSameLine: true,
};
