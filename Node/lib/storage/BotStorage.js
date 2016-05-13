var MemoryBotStorage = (function () {
    function MemoryBotStorage() {
        this.userStore = {};
        this.conversationStore = {};
    }
    MemoryBotStorage.prototype.get = function (address, callback) {
        var data = {};
        if (address.userId) {
            if (this.userStore.hasOwnProperty(address.userId)) {
                data.userData = JSON.parse(this.userStore[address.userId]);
            }
            else {
                data.userData = null;
            }
        }
        if (address.conversationId) {
            if (this.conversationStore.hasOwnProperty(address.conversationId)) {
                data.conversationData = JSON.parse(this.conversationStore[address.conversationId]);
            }
            else {
                data.conversationData = null;
            }
        }
        callback(null, data);
    };
    MemoryBotStorage.prototype.save = function (address, data, callback) {
        if (address.userId) {
            this.userStore[address.userId] = JSON.stringify(data.userData || {});
        }
        if (address.conversationId) {
            this.conversationStore[address.conversationId] = JSON.stringify(data.conversationData || {});
        }
    };
    MemoryBotStorage.prototype.delete = function (address) {
        if (address.userId && this.userStore.hasOwnProperty(address.userId)) {
            delete this.userStore[address.userId];
        }
        if (address.conversationId && this.conversationStore.hasOwnProperty(address.conversationId)) {
            delete this.conversationStore[address.conversationId];
        }
    };
    return MemoryBotStorage;
})();
exports.MemoryBotStorage = MemoryBotStorage;
