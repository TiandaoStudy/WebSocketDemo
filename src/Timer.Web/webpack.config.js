var isProduction = process.env.NODE_ENV === 'production';

if (isProduction) {
    module.exports = require('./webpack.production.js');
} else {
    module.exports = require('./webpack.development.js');
}