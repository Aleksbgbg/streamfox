module.exports = {
  productionSourceMap: false,
  devServer: {
    host: "localhost",
    port: 8080
  },
  publicPath: process.env.VUE_APP_PUBLIC_PATH
};