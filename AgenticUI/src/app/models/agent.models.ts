export interface AgentRunRequest {
  prompt: string;
}

export interface AgentRunResponse {
  result: string;
  steps: string[];
}

export interface ChatMessage {
  role: 'user' | 'agent';
  content: string;
  steps?: string[];
  timestamp: Date;
}
