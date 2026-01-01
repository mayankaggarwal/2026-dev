using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace mydev.MCPSample.Api.TaskManagement
{
    [McpServerToolType]
    public static class TaskEndpoints
    {
        public static void Add(RouteGroupBuilder api)
        {
            var tasks = api.MapGroup("/tasks");
            tasks.MapGet("/", GetAllTasks).WithName("GetTasks").WithOpenApi();
            tasks.MapGet("/{taskId}", GetTaskById).WithName("GetTaskById").WithOpenApi();
            tasks.MapPost("/", CreateTask).WithName("CreateTask").WithOpenApi();
        }

        [McpServerTool(Name = "create_mytask"), Description("Create the task for the application.")]
        public static async Task<MyTask> CreateTask(
            [Description("Title of the task")] string title,
            [Description("Status of the task")] string? status = null)
        {
            var task = new MyTask
            {
                Id = new Random().Next(100, 999),
                Title = title,
                Status = status ?? "Pending"
            };
            return task;
        }

        [McpServerTool(Name = "get_mytask_by_id"), Description("Get a specific task by ID")]
        public static async Task<MyTask> GetTaskById(
            [FromRoute,Description("The ID of the task to retrieve")] int taskId)
        {
            return new() { Id = taskId, Title = $"Task {taskId}", Status = "Active" };
        }

        [McpServerTool(Name = "get_mytask_list"), Description("Get all tasks for the application.")]
        public static async Task<MyTask[]> GetAllTasks()
        {
            var tasks = new MyTask[]
            {
                new() { Id = 1, Title = "Complete project", Status = "In Progress" },
                new() { Id = 2, Title = "Review code", Status = "Pending" },
                new() { Id = 3, Title = "Deploy to production", Status = "Completed" }
            };

            return tasks;
        }
    }

    public class MyTask
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Status { get; set; }
    }
}
