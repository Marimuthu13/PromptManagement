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
  isFallback?: boolean;
}

export interface PromptVersionDto {
  id: string;
  promptId: string;
  content: string;
  versionNumber: number;
  createdAtUtc: string;
}

export interface RecentExecutionDto {
  featureType: string;
  displayTitle: string;
  modelUsed: string;
  totalTokens: number;
  latencyMs: number;
  timestamp: string;
  status: string;
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

export interface PromptAnalysisDto {
  score: number;
  grade: string;
  suggestions: string[];
  optimizedPrompt: string;
  isFallback?: boolean;
}
