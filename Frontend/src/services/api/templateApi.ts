import { apiClient } from './apiClient';
import type { TemplateDto, GetTemplatesRequest } from '../types/template';
import type { PagedResult } from '../types/common';

export const templateApi = {
  getTemplates: async (params: GetTemplatesRequest): Promise<PagedResult<TemplateDto>> => {
    const query = new URLSearchParams();
    
    if (params.page) query.append('page', params.page.toString());
    if (params.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params.categoryId) query.append('categoryId', params.categoryId);
    if (params.searchText) query.append('search', params.searchText);

    const response = await apiClient.get<PagedResult<TemplateDto>>(`/api/templates?${query.toString()}`);
    return response.data;
  },

  duplicateTemplate: async (id: string): Promise<string> => {
    const response = await apiClient.post<string>(`/api/templates/${id}/duplicate`);
    return response.data;
  }
};
