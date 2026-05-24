import { Component, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AgentService } from './services/agent.service';
import { ChatMessage } from './models/agent.models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
<div class="bg-glow">
  <div class="nebula"></div>
  <div class="particles"></div>
</div>

<div class="chat-container">
  <header>
    <h1>Agentic Framework</h1>
    <p>Autonomous Tool Orchestration Engine</p>
  </header>

  <main #scrollContainer>
    <div *ngFor="let msg of messages" class="message" 
         [ngClass]="msg.role === 'user' ? 'user' : 'agent'">
      
      <p style="white-space: pre-wrap; margin-bottom: 0;">{{ msg.content }}</p>

      <div *ngIf="msg.steps && msg.steps.length > 0" class="reasoning-container">
        <span class="reasoning-label">Agent Thought Process Trace</span>
        <div class="code-block">
          <ul>
            <li *ngFor="let step of msg.steps">{{ step }}</li>
          </ul>
        </div>
      </div>
      
      <span class="timestamp">
        {{ msg.timestamp | date:'shortTime' }}
      </span>
    </div>

    <div *ngIf="isLoading" class="loading-indicator">
      <div class="dot-pulse">
        <div class="dot"></div>
        <div class="dot"></div>
        <div class="dot"></div>
      </div>
      <span>Agent is thinking...</span>
    </div>
  </main>

  <footer>
    <form (submit)="sendMessage()" class="input-wrapper">
      <input type="text" 
             [(ngModel)]="prompt" 
             name="prompt"
             placeholder="Enter objective (e.g., 'Check weather in London')" 
             [disabled]="isLoading"
             autocomplete="off">
      
      <button type="submit" 
              class="send-btn"
              [disabled]="isLoading || !prompt.trim()">
        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
          <path d="M15.854.146a.5.5 0 0 1 .11.54l-5.819 14.547a.75.75 0 0 1-1.329.124l-3.178-4.995L.643 7.184a.75.75 0 0 1 .124-1.33L15.314.037a.5.5 0 0 1 .54.11ZM6.636 10.07l2.761 4.338L14.13 2.576 6.636 10.07Zm6.787-8.201L1.591 6.602l4.339 2.76 7.493-7.493Z"/>
        </svg>
      </button>
    </form>
  </footer>
</div>
  `,
  styleUrls: ['./app.css']
})
export class AppComponent implements AfterViewChecked {
  @ViewChild('scrollContainer') private scrollContainer!: ElementRef;

  prompt: string = '';
  isLoading: boolean = false;
  messages: ChatMessage[] = [
    {
      role: 'agent',
      content: 'Hello! I am your AI assistant. How can I help you today?',
      timestamp: new Date()
    }
  ];

  constructor(private agentService: AgentService) {}

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  private scrollToBottom(): void {
    try {
      this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
    } catch(err) { }
  }

  sendMessage() {
    if (!this.prompt.trim() || this.isLoading) return;

    const userPrompt = this.prompt.trim();
    this.messages.push({
      role: 'user',
      content: userPrompt,
      timestamp: new Date()
    });

    this.prompt = '';
    this.isLoading = true;

    this.agentService.runAgent(userPrompt).subscribe({
      next: (response) => {
        this.messages.push({
          role: 'agent',
          content: response.result,
          steps: response.steps,
          timestamp: new Date()
        });
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error calling agent:', error);
        this.messages.push({
          role: 'agent',
          content: 'Sorry, I encountered an error while processing your request.',
          timestamp: new Date()
        });
        this.isLoading = false;
      }
    });
  }
}
