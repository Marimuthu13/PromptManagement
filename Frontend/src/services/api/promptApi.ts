import { apiClient } from './apiClient';
import type { PromptDto, CreatePromptRequest, UpdatePromptRequest, ExecutePromptRequest, PromptExecutionDto, PromptVersionDto, GetPromptsRequest, TokenAnalyticsDto, PromptAnalysisDto } from '../types/prompt';
import type { PagedResult } from '../types/common';

export const promptApi = {
  getPrompts: async (params: GetPromptsRequest): Promise<PagedResult<PromptDto>> => {
    const query = new URLSearchParams();
    
    if (params.page) query.append('page', params.page.toString());
    if (params.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params.categoryId) query.append('categoryId', params.categoryId);
    if (params.searchText) query.append('search', params.searchText);

    const response = await apiClient.get<PagedResult<PromptDto>>(`/api/prompts?${query.toString()}`);
    return response.data;
  },

  getPromptById: async (id: string): Promise<PromptDto> => {
    const response = await apiClient.get<PromptDto>(`/api/prompts/${id}`);
    return response.data;
  },

  createPrompt: async (data: CreatePromptRequest): Promise<{ id: string }> => {
    const response = await apiClient.post<{ id: string }>('/api/prompts', data);
    return response.data;
  },

  updatePrompt: async (id: string, data: UpdatePromptRequest): Promise<PromptDto> => {
    const response = await apiClient.put<PromptDto>(`/api/prompts/${id}`, data);
    return response.data;
  },

  deletePrompt: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/prompts/${id}`);
  },

  executePrompt: async (id: string, data: ExecutePromptRequest): Promise<PromptExecutionDto> => {
    const response = await apiClient.post<PromptExecutionDto>(`/api/Prompts/${id}/execute`, data);
    return response.data;
  },

  getPromptExecutions: async (id: string): Promise<PromptExecutionDto[]> => {
    const response = await apiClient.get<PromptExecutionDto[]>(`/api/prompts/${id}/executions`);
    return response.data;
  },

  getPromptVersions: async (id: string): Promise<PromptVersionDto[]> => {
    const response = await apiClient.get<PromptVersionDto[]>(`/api/prompts/${id}/versions`);
    return response.data;
  },

  restorePromptVersion: async (id: string, versionId: string): Promise<PromptDto> => {
    const response = await apiClient.post<PromptDto>(`/api/prompts/${id}/versions/${versionId}/restore`);
    return response.data;
  },

  getAnalytics: async (): Promise<TokenAnalyticsDto> => {
    const response = await apiClient.get<TokenAnalyticsDto>('/api/analytics/tokens');
    return response.data;
  },

  analyzeImage: async (imageData: string): Promise<{ result: string }> => {
    const response = await apiClient.post<{ result: string }>('/api/vision/analyze', { imageData });
    return response.data;
  },

  optimizePrompt: async (draftContent: string): Promise<PromptAnalysisDto> => {
    const response = await apiClient.post<PromptAnalysisDto>('/api/prompts/optimize', { draftContent });
    return response.data;
  },

  generateVariables: async (promptContent: string, variableNames: string[]): Promise<Record<string, string>> => {
    const response = await apiClient.post<Record<string, string>>('/api/prompts/generate-variables', { promptContent, variableNames });
    return response.data;
  }
};
