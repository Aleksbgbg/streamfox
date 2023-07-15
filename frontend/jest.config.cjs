module.exports = {
  moduleFileExtensions: ["js", "ts", "json", "vue"],
  moduleNameMapper: {
    "@/(.*)": "<rootDir>/src/$1",
  },
  transform: {
    "^.+\\.ts$": [
      "ts-jest",
      {
        tsconfig: "./jest.tsconfig.json",
      },
    ],
    "^.+\\.vue$": "@vue/vue3-jest",
  },
};
