import type { PagedRequest } from './common';

export interface TemplateDto {
  id: string;
  title: string;
  description: string;
  content: string;
  categoryId: string;
  categoryName: string;
  isSystemCurated: boolean;
}

export interface GetTemplatesRequest extends PagedRequest {
  categoryId?: string;
  searchText?: string;
}
