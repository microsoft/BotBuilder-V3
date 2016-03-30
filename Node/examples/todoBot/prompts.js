module.exports = {
    helpMessage: 'Available commands are:\n\n' +
    '* *list* - show all tasks (also *show*, *tasks*)\n' +
    '* *new* [your task] - create a new task (also *save*, *create*, *add*)\n' +
    '* *done* [number of task] - finish a task, you can get the number of a task from *list* (also *delete", *finish*, *remove*)',
    saveTaskCreated: 'Created a new task no. %(index)d: %(task)s',
    saveTaskMissing: 'You need to tell me what the new task should be. For example: "new Remember the milk"',
    listTaskList: 'Tasks:\n%s',
    listTaskItem: '%(index)d. %(task)s\n',
    listNoTasks: 'You have no tasks.',
    finishTaskMissing: 'You need to specify the number of the task you want to finish.',
    finishTaskInvalid: 'There is no task no. %(index)d',
    finishTaskDone: 'Task no. %(index)d done.'
};
