const path = require("path");
const { defineConfig } = require("@vue/cli-service");
const webpack = require("webpack");

module.exports = defineConfig({
    transpileDependencies: true,
    publicPath: "./",

    configureWebpack: {
        devServer: {
            historyApiFallback: true,
            open: true,
        },
        plugins: [
            new webpack.DefinePlugin({
                __VUE_PROD_HYDRATION_MISMATCH_DETAILS__: "false",
            }),
        ],
    },
    pages: {
        index: {
            entry: "src/main.js",
        },
    },
    chainWebpack: (config) => {
        config.resolve.alias.set("vue$", "vue/dist/vue.runtime.esm-bundler.js");
        config.resolve.alias.set("vue-i18n$", "vue-i18n/dist/vue-i18n.runtime.esm-bundler.js");
        config.module.rule("vue").use("vue-svg-inline-loader").loader("vue-svg-inline-loader").options({
            /* ... */
        });
    },
});
