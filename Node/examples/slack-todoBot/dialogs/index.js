var builder = require('../../../');
var prompts = require('../prompts');

// Export Command Dialog
module.exports = new builder.CommandDialog()
    .matches('^(hello|hi|howdy|help)', builder.DialogAction.send(prompts.helpMessage))
    .matches('^(?:new|save|create|add)(?: (.+))?', saveTask)
    .matches('^(?:done|delete|finish|remove)(?: (\\d+))?', finishTask)
    .matches('^(list|show|tasks)', listTasks);

function saveTask(session, args) {
    if (args.matches) {
        var task = args.matches[1];
        if (task) {
            var entry = { task: task, user: session.message.from.address };
            if (!session.channelData.tasks) {
                session.channelData.tasks = [entry];
            }
            else {
                session.channelData.tasks.push(entry);
            }
            session.send(prompts.saveTaskCreated, { index: session.channelData.tasks.length, task: task });
        }
        else {
            session.send(prompts.saveTaskMissing);
        }
    }
}

function finishTask(session, args) {
    if (args.matches) {
        var taskNumber = Number(args.matches[1]) - 1;
        if (isNaN(taskNumber)) {
            return session.send(prompts.finishTaskMissing);
        }
        if (!session.channelData.tasks) {
            session.channelData.tasks = [];
        }
        if (session.channelData.tasks.length <= taskNumber || taskNumber < 0) {
            session.send(prompts.finishTaskInvalid, { index: taskNumber + 1 });
        }
        else {
            session.channelData.tasks = session.channelData.tasks.slice(0, taskNumber).concat(session.channelData.tasks.slice(taskNumber + 1));
            session.send(prompts.finishTaskDone, { index: taskNumber + 1 });
            listTasks(session, null);
        }
    }
    else {
        session.send(prompts.finishTaskMissing);
    }
}

function listTasks(session, args) {
    if (!session.channelData.tasks) {
        session.channelData.tasks = [];
    }
    if (session.channelData.tasks.length > 0) {
        var list = '';
        session.channelData.tasks.forEach(function (value, index) {
            list += session.gettext(prompts.listTaskItem, { index: index + 1, task: value.task, user: value.user });
        });
        session.send(prompts.listTaskList, list);
    }
    else {
        session.send(prompts.listNoTasks);
    }
}
