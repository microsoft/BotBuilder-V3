function maxButtons(session) {
    var account = session.message.from || session.message.to;
    switch (account.channelId.toLowerCase()) {
        case 'facebook':
            return 3;
        case 'telegram':
        case 'kik':
            return 100;
        default:
            return 0;
    }
}
exports.maxButtons = maxButtons;
