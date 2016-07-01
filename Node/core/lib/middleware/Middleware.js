var Middleware = (function () {
    function Middleware() {
    }
    Middleware.dialogVersion = function (options) {
        return {
            dialog: function (session, next) {
                var cur = session.sessionState.version || 0.0;
                var curMajor = Math.floor(cur);
                var major = Math.floor(options.version);
                if (session.sessionState.callstack.length && curMajor !== major) {
                    session.endConversation(options.message || "Sorry. The service was upgraded and we need to start over.");
                }
                else if (options.resetCommand && session.message.text && options.resetCommand.test(session.message.text)) {
                    session.endConversation(options.message || "Sorry. The service was upgraded and we need to start over.");
                }
                else {
                    session.sessionState.version = options.version;
                    next();
                }
            }
        };
    };
    return Middleware;
})();
exports.Middleware = Middleware;
