import type { PagedRequest } from './common';

export interface PromptVariableDto {
  id: string;
  name: string;
  isRequired: boolean;
}

export interface PromptDto {
  id: string;
  title: string;
  description: string;
  content: string;
  categoryId: string;
  userId: string;
  executionCount?: number;
  variables: PromptVariableDto[];
}

export interface GetPromptsRequest extends PagedRequest {
  categoryId?: string;
  searchText?: string;
}

export interface CreatePromptRequest {
  title: string;
  description: string;
  content: string;
  categoryId: string;
  isTemplate?: boolean;
}

export interface UpdatePromptRequest {
  id: string;
  title: string;
  description: string;
  content: string;
  categoryId: string;
}

export interface ExecutePromptRequest {
  promptId: string;
  providerName: string;
  modelName: string;
  variables: Record<string, string>;
}

export interface PromptExecutionDto {
  id: string;
  promptId: string;
  provider: string;
  model: string;
  resultContent: string;
  tokensUsed: number;
  durationMs: number;
  isSuccessful: boolean;
  errorMessage?: string;
  createdAtUtc: string;
}

export interface PromptVersionDto {
  id: string;
  promptId: string;
  content: string;
  versionNumber: number;
  createdAtUtc: string;
}

export interface RecentExecutionDto {
  title: string;
  timestamp: string;
  totalTokens: number;
}

export interface TopPromptDto {
  promptId: string;
  title: string;
  totalTokens: number;
}

export interface TokenAnalyticsDto {
  totalSystemTokens: number;
  recentExecutions: RecentExecutionDto[];
  topPromptsByUsage: TopPromptDto[];
}
