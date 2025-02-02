const path = require('path');
const { VueLoaderPlugin } = require('vue-loader');
const HtmlWebpackPlugin = require('html-webpack-plugin');

module.exports = {
    mode: 'development', // or 'production' for production builds
    entry: './src/main.js', // Entry point for your app
    output: {
        path: path.resolve(__dirname, 'wwwroot/js'),
        filename: 'resistance.js', // The bundled file
    },
    module: {
        rules: [
            {
                test: /\.vue$/,
                loader: 'vue-loader',
            },
            {
                test: /\.js$/,
                loader: 'babel-loader',
                exclude: /node_modules/,
                options: {
                    presets: ['@babel/preset-env'],
                },
            },
            {
                test: /\.css$/,
                use: ['vue-style-loader', 'css-loader'],
            },
        ],
    },
    plugins: [
        new VueLoaderPlugin(),
        new HtmlWebpackPlugin({
            template: './Views/Shared/_Layout.cshtml',
            filename: '../../Views/Shared/_Layout.cshtml', // To update Layout.cshtml with bundled script
            inject: false, // Prevent automatic script injection, as ASP.NET handles it differently
        }),
    ],
    resolve: {
        alias: {
            vue$: 'vue/dist/vue.esm-bundler.js',
        },
        extensions: ['.js', '.vue', '.json'],
    },
    devtool: 'source-map', // Helpful for debugging
};
