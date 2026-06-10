# 🚀 RgAi AI Agent IDE - Professional Frontend Guide

## ✅ Frontend is Now Running!

Your professional developer interface is live at: **http://localhost:3000**

---

## 🎯 What You Get

A production-ready, developer-focused AI agent IDE with:

| Feature | Details |
|---------|---------|
| **Real-time Chat** | Stream responses from Qwen3-Coder LLM |
| **Code Generation** | Generate production-ready code |
| **Agent Modes** | Chat, Code, Debug, Explain modes |
| **Live Syntax** | Automatic code block detection |
| **Connection Monitor** | Real-time API status |
| **Agent State** | Visual feedback on AI processing |
| **Code Viewer** | Syntax-highlighted code display |
| **Copy to Clipboard** | One-click code copying |
| **Professional UI** | Dark theme optimized for developers |
| **Responsive Design** | Works on desktop, tablet, mobile |

---

## 🎨 Interface Overview

```
┌─────────────────────────────────────────────────────────────────┐
│  🚀 RgAi - AI Agent IDE                      🟢 Connected      │
└─────────────────────────────────────────────────────────────────┘
│                                                                   │
│  ┌──────────────────────────────────────┐  ┌──────────────────┐ │
│  │                                      │  │ Agent Status     │ │
│  │          CHAT MESSAGES               │  │ 🤖 Processing   │ │
│  │                                      │  │                  │ │
│  │  👤 User                             │  │ Statistics       │ │
│  │  > "Generate Python function"        │  │ Messages: 5      │ │
│  │                                      │  │ Code Blocks: 2   │ │
│  │  🤖 Agent                            │  │                  │ │
│  │  ```python                           │  │ Code Viewer      │ │
│  │  def merge_arrays(a, b):             │  │ ┌──────────────┐ │ │
│  │      return sorted(a + b)            │  │ │def function()│ │ │
│  │  ```                                 │  │ │    ...       │ │ │
│  │                                      │  │ └──────────────┘ │ │
│  └──────────────────────────────────────┘  │                  │ │
│                                            │ How to Use       │ │
│  Mode: [Chat ▼]                           │ Tips & Tricks    │ │
│  ┌──────────────────────────────────────┐  └──────────────────┘ │
│  │ Your prompt here... (Shift+Enter)    │                        │
│  │                                      │                        │
│  │ [📤 Send]  [🗑️ Clear]                │                        │
│  └──────────────────────────────────────┘                        │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Interaction Modes

### 1. **Chat Mode** (Default)
- General questions and conversations
- Technical discussions
- Brainstorming and planning
```
Prompt: "Explain async/await in JavaScript"
Response: Full explanation with examples
```

### 2. **Code Mode**
- Generate production-ready code
- Implement algorithms
- Create functions and classes
```
Prompt: "Create a REST API endpoint for authentication"
Response: Complete code with comments
```

### 3. **Debug Mode**
- Fix bugs and errors
- Analyze code issues
- Provide solutions
```
Prompt: "Why is my loop not working? for(let i=0; i<10, i++)"
Response: Identifies the error and explains the fix
```

### 4. **Explain Mode**
- Understand code snippets
- Learn concepts
- Break down complex logic
```
Prompt: "Explain this function: async function* generator() { yield 42; }"
Response: Step-by-step explanation
```

---

## 💻 How to Use

### 1. **Type Your Prompt**
Write your question or request in the text area:
```
"Write a Python function to calculate Fibonacci numbers"
```

### 2. **Select Mode** (Optional)
Choose the appropriate mode from the dropdown:
- Chat (general questions)
- Code (code generation)
- Debug (fixing issues)
- Explain (understanding code)

### 3. **Send the Request**
- Click **📤 Send** button, OR
- Press **Enter** (automatic send)
- Press **Shift+Enter** for new line in the text area

### 4. **Get Response**
Watch the AI agent process:
1. Status shows **🧠 Processing**
2. Agent generates response
3. Status shows **✅ Complete**
4. Response appears in chat with code blocks highlighted

### 5. **Interact with Code**
- See code badges (Python, JavaScript, etc.)
- Click on code blocks to view in Code Viewer
- Copy code with **📋 Copy** button
- Use code in your project

---

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Enter` | Send message |
| `Shift+Enter` | New line in text area |
| `Ctrl+L` | Clear chat |

---

## 🎯 Use Cases & Examples

### Example 1: Generate a Function
```
Mode: Code
Prompt: "Create a Python function to merge two sorted arrays efficiently"

Response:
```python
def merge_sorted_arrays(arr1, arr2):
    """Merge two sorted arrays with O(n+m) complexity."""
    result = []
    i = j = 0
    
    while i < len(arr1) and j < len(arr2):
        if arr1[i] <= arr2[j]:
            result.append(arr1[i])
            i += 1
        else:
            result.append(arr2[j])
            j += 1
    
    result.extend(arr1[i:])
    result.extend(arr2[j:])
    return result
```
```

### Example 2: Debug Code
```
Mode: Debug
Prompt: "Fix this JavaScript error: TypeError: Cannot read property 'map' of undefined"

Response: Shows the issue and provides corrected code
```

### Example 3: Explain Code
```
Mode: Explain
Prompt: "Explain this async function"

Response: Breaks down each line with explanations
```

### Example 4: General Chat
```
Mode: Chat
Prompt: "What are the best practices for writing clean code?"

Response: Comprehensive discussion with examples
```

---

## 📊 Agent Panel

### Status Indicator
- **⏸️ Idle**: Ready for input
- **🧠 Processing**: AI is thinking
- **✅ Complete**: Response ready
- **❌ Error**: Something went wrong

### Statistics
- **Messages**: Total messages in conversation
- **Current State**: What the agent is doing
- **Code Blocks**: Detected code in responses

### Pro Tips
- Be specific with requirements
- Include error messages for debugging
- Ask for explanations to learn
- Use code blocks for best formatting

---

## 🔗 Backend API Endpoints

The frontend uses these backend endpoints:

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/health` | GET | Check if API is running |
| `/status` | GET | Get detailed service status |
| `/test-inference` | POST | Run LLM inference |
| `/ollama/models` | GET | List available models |
| `/ollama/pull` | POST | Pull a new model |

---

## 🚀 Tips & Tricks

### For Better Responses:
1. **Be Specific**: "Create a function that..." works better than "make a thing"
2. **Include Context**: Share error messages, code snippets, requirements
3. **Use Mode**: Select appropriate mode for your task
4. **Ask Follow-ups**: "Explain more", "Add error handling", etc.
5. **Reference Code**: Paste problematic code for debugging

### For Code Generation:
1. Specify language explicitly: "Python function..."
2. Include requirements: "Production-ready, with error handling"
3. Ask for tests: "Also write unit tests"
4. Request comments: "Add inline comments"

### For Debugging:
1. Include full error message
2. Paste the failing code
3. Describe what you expected
4. Mention environment if relevant

---

## 🔧 Configuration

### Backend Settings
Located in `.env`:

```env
# LLM Configuration
LLM_HOST=llm
LLM_PORT=11434
LLM_MODEL=qwen3-coder

# Backend
BACKEND_HOST=0.0.0.0
BACKEND_PORT=8000

# Logging
LOG_LEVEL=INFO
```

### Frontend Configuration
Located in `frontend/` directory:
- Component-based architecture
- Modular CSS styling
- Real-time status updates
- Responsive design

---

## 📱 Responsive Design

The frontend works on:
- **Desktop**: Full 2-column layout
- **Tablet**: Stacked layout with scrolling
- **Mobile**: Optimized single column

---

## 🔐 Features

✅ **Real-time API Connection Monitoring**
- Shows 🟢 Connected or 🔴 Disconnected
- Auto-reconnect functionality
- Connection warnings

✅ **Code Detection & Formatting**
- Automatically finds code blocks
- Language detection (Python, JavaScript, etc.)
- Syntax highlighting
- Copy to clipboard

✅ **Agent State Management**
- Visual feedback on processing
- Status animations
- Error handling with messages

✅ **Professional UI/UX**
- Dark theme optimized for coding
- Smooth animations
- Accessibility features
- Keyboard navigation

---

## 📞 Troubleshooting

### Frontend Shows "🔴 Disconnected"
- Check if backend is running: `docker compose ps`
- Verify port 8000 is accessible
- Restart backend: `docker compose restart backend`

### Responses are Slow
- Check LLM container is running
- Model may be loading for first time
- Check system resources: `docker stats`

### Code Not Showing in Viewer
- Ensure code is in markdown blocks: ` ```language code``` `
- Try a different prompt
- Check browser console for errors

### Frontend Not Loading
- Check if it's running: `docker compose ps`
- Verify port 3000 is accessible
- Try `docker compose restart frontend`

---

## 🎓 Learning Path

1. **Start with Chat Mode**
   - Get comfortable with interface
   - See how AI responds

2. **Try Code Mode**
   - Generate simple functions
   - See code quality

3. **Experiment with All Modes**
   - Debug your own code
   - Explain complex concepts
   - Generate new projects

4. **Advanced Usage**
   - Iterate on responses
   - Build complete systems
   - Use as learning tool

---

## 🚀 Next Steps

1. ✅ **Verify Frontend**: http://localhost:3000
2. 📝 **Try Sample Prompts** (available in empty state)
3. 🎯 **Generate Your First Code**
4. 🔄 **Experiment with Modes**
5. 🧠 **Use as Learning Tool**

---

## 📊 Service Status

All services running at:
- **Frontend**: http://localhost:3000 ✅
- **Backend API**: http://localhost:8000 ✅
- **API Docs**: http://localhost:8000/docs ✅
- **Ollama LLM**: http://localhost:11434 ✅
- **Qdrant DB**: http://localhost:6333 ✅

---

## 🎉 You're Ready!

Your professional AI agent IDE is fully operational. Start asking questions, generating code, and debugging with AI assistance.

Visit **http://localhost:3000** now!
