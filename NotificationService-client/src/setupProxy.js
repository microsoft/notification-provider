const { createProxyMiddleware } = require('http-proxy-middleware');

module.exports = function(app) {
    app.use(
        '/v1/report',
        createProxyMiddleware({
            target: '__notificationServiceUrl__',
            changeOrigin: true,
        })
    );
    app.use(
        '/v1/email',
        createProxyMiddleware({
            target: '__notificationHandlerUrl__',
            changeOrigin: true,
            secure: false
        })
    );
    app.use(
        '/v1/client',
        createProxyMiddleware({
            target: '__notificationHandlerUrl__',
            changeOrigin: true,
            secure: false
        })
    );
};