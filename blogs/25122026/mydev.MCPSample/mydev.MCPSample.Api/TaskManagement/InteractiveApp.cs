
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace mydev.MCPSample.Api.TaskManagement
{
    [McpServerToolType]
    public static class InteractiveApp
    {
        // 1. THE UI RESOURCE (The interactive form)
        [McpServerResource(IconSource = "ui://forms/input-collector")]
        public static string GetInputForm()
        {
            return """
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body { font-family: sans-serif; padding: 10px; color: white; background: #1e1e1e; }
                input { width: 100%; margin: 5px 0; padding: 8px; border-radius: 4px; border: 1px solid #333; }
                button { background: #007acc; color: white; border: none; padding: 10px; cursor: pointer; width: 100%; }
            </style>
        </head>
        <body>
            <h3>Submit Data to Tool</h3>
            <input type="text" id="userName" placeholder="Enter Name">
            <input type="number" id="userAge" placeholder="Enter Age">
            <button onclick="submitData()">Send to Copilot</button>

            <script>
                async dreaming() {
                    const name = document.getElementById('userName').value;
                    const age = parseInt(document.getElementById('userAge').value);
                    
                    // The 'mcp' object is injected into the iframe by Copilot
                    try {
                        const result = await window.mcp.callTool('ProcessUserData', { 
                            name: name, 
                            age: age 
                        });
                        alert('Success: ' + JSON.stringify(result));
                    } catch (err) {
                        console.error('Tool call failed', err);
                    }
                }
                function submitData() { dreaming(); }
            </script>
        </body>
        </html>
        """;
        }

        // 2. THE TRIGGER (Launches the UI)
        [McpServerTool, Description("Opens a form to collect user information.")]
        public static object OpenDataForm()
        {
            // We use a Dictionary to ensure the JSON structure is exactly:
            // { "content": [...], "_meta": { "ui": { "resourceUri": "..." } } }
            return new Dictionary<string, object>
            {
                ["content"] = new[] {
                new { type = "text", text = "Please fill out the form below to proceed:" }
            },
                ["_meta"] = new
                {
                    ui = new { resourceUri = "ui://forms/input-collector" }
                }
            };
        }

        // 3. THE PROCESSOR (Receives the form data)
        [McpServerTool, Description("Processes the data submitted from the UI form.")]
        public static string ProcessUserData(string name, int age)
        {
            // This is where your backend logic happens
            return $"Successfully received {name} (Age: {age}). Data has been logged.";
        }
    }
}
