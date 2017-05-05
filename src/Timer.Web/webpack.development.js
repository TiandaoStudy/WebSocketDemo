var CleanWebpackPlugin = require('clean-webpack-plugin');
var HtmlWebpackPlugin = require('html-webpack-plugin');
var webpack = require('webpack');

var path = require('path');

var rootPath = path.resolve(__dirname, "wwwroot");

module.exports = {
    entry: {
        main: './Scripts/index.ts',
        callback: './Scripts/callback.ts',
        vendor: ['jquery', 'moment', 'oidc-client']
    },
    output: {
        filename: '[name].[hash].js',
        chunkFilename: '[id].[hash].chunk.js',
        path: path.resolve(rootPath, "scripts/dist")
    },
    resolve: {
        extensions: ['.ts', '.tsx', '.js']
    },
    module: {
        loaders: [{ test: /\.tsx?$/, loader: 'ts-loader' }]
    },
    plugins: [
        new CleanWebpackPlugin(
            [rootPath],
            {
                watch: true
            }),
        new webpack.optimize.CommonsChunkPlugin({
            name: 'vendor'
        }),
        new HtmlWebpackPlugin({
            template: './App/index.html',
            chunks: ['main', 'vendor'],
            inject: 'body',
            filename: path.resolve(rootPath, 'index.html')
        }),
        new HtmlWebpackPlugin({
            template: './App/callback.html',
            chunks: ['callback', 'vendor'],
            inject: 'body',
            filename: path.resolve(rootPath, 'callback.html')
        })
    ]
}