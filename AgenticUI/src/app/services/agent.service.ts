import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AgentRunRequest, AgentRunResponse } from '../models/agent.models';

@Injectable({
  providedIn: 'root'
})
export class AgentService {
  private apiUrl = 'http://localhost:5000/api/agent/run';

  constructor(private http: HttpClient) {}

  runAgent(prompt: string): Observable<AgentRunResponse> {
    const request: AgentRunRequest = { prompt };
    return this.http.post<AgentRunResponse>(this.apiUrl, request);
  }
}
