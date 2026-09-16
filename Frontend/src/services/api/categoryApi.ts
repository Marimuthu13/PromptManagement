import { apiClient } from './apiClient';
import type { CategoryDto } from '../types/category';

export const categoryApi = {
  getCategories: async (): Promise<CategoryDto[]> => {
    const response = await apiClient.get<CategoryDto[]>('/api/categories');
    return response.data;
  },
  createCategory: async (name: string): Promise<string> => {
    const response = await apiClient.post<string>('/api/categories', { name });
    return response.data;
  },
  updateCategory: async (id: string, name: string): Promise<void> => {
    await apiClient.put(`/api/categories/${id}`, { id, name });
  },
  deleteCategory: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/categories/${id}`);
  },
};
