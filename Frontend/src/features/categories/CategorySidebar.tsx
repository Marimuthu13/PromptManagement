import React, { useEffect, useState } from 'react';
import type { CategoryDto } from '../../services/types/category';
import { categoryApi } from '../../services/api/categoryApi';

interface CategorySidebarProps {
  selectedCategoryId: string | undefined;
  onSelectCategory: (categoryId: string | undefined) => void;
}

const CategorySidebar: React.FC<CategorySidebarProps> = ({ selectedCategoryId, onSelectCategory }) => {
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const [isCreatingCategory, setIsCreatingCategory] = useState(false);
  const [newCategoryName, setNewCategoryName] = useState('');

  const [editingCategoryId, setEditingCategoryId] = useState<string | null>(null);
  const [editCategoryName, setEditCategoryName] = useState('');

  const fetchCategories = async () => {
    try {
      const data = await categoryApi.getCategories();
      setCategories(data);
    } catch (err) {
      setError('Failed to load categories');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchCategories();
  }, []);

  const handleCreateCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newCategoryName.trim()) return;
    
    try {
      await categoryApi.createCategory(newCategoryName);
      setNewCategoryName('');
      setIsCreatingCategory(false);
      fetchCategories();
    } catch (err) {
      alert('Failed to create category');
    }
  };

  const handleEditStart = (category: CategoryDto) => {
    setEditingCategoryId(category.id);
    setEditCategoryName(category.name);
  };

  const handleEditSave = async (id: string, e: React.FormEvent) => {
    e.preventDefault();
    if (!editCategoryName.trim()) return;
    
    try {
      await categoryApi.updateCategory(id, editCategoryName);
      setEditingCategoryId(null);
      setEditCategoryName('');
      fetchCategories();
    } catch (err) {
      alert('Failed to update category');
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm("Are you sure you want to delete this category?")) {
      try {
        await categoryApi.deleteCategory(id);
        if (selectedCategoryId === id) {
          onSelectCategory(undefined);
        }
        fetchCategories();
      } catch (err) {
        alert('Failed to delete category');
      }
    }
  };

  if (isLoading) return <div className="sidebar-loading">Loading categories...</div>;
  if (error) return <div className="sidebar-error">{error}</div>;

  return (
    <div className="category-sidebar">
      <div className="category-sidebar-header">
        <h3>Categories</h3>
        <button className="add-category-btn" onClick={() => setIsCreatingCategory(!isCreatingCategory)}>+</button>
      </div>

      {isCreatingCategory && (
        <form className="category-create-form" onSubmit={handleCreateCategory}>
          <input 
            type="text" 
            placeholder="New category..." 
            value={newCategoryName}
            onChange={(e) => setNewCategoryName(e.target.value)}
            autoFocus
          />
          <div className="category-create-actions">
            <button type="submit" className="save-cat-btn">Save</button>
            <button type="button" className="cancel-cat-btn" onClick={() => { setIsCreatingCategory(false); setNewCategoryName(''); }}>Cancel</button>
          </div>
        </form>
      )}

      <ul className="category-list">
        <li 
          className={!selectedCategoryId ? 'active' : ''}
          onClick={() => onSelectCategory(undefined)}
        >
          All Prompts
        </li>
        {categories.map((category) => (
          <li 
            key={category.id}
            className={`category-item ${selectedCategoryId === category.id ? 'active' : ''}`}
            onClick={() => onSelectCategory(category.id)}
          >
            {editingCategoryId === category.id ? (
              <form className="category-create-form" style={{ width: '100%', marginBottom: 0, paddingBottom: 0, borderBottom: 'none' }} onSubmit={(e) => handleEditSave(category.id, e)} onClick={(e) => e.stopPropagation()}>
                <input 
                  type="text" 
                  value={editCategoryName}
                  onChange={(e) => setEditCategoryName(e.target.value)}
                  autoFocus
                />
                <div className="category-create-actions">
                  <button type="submit" className="save-cat-btn">Save</button>
                  <button type="button" className="cancel-cat-btn" onClick={() => setEditingCategoryId(null)}>Cancel</button>
                </div>
              </form>
            ) : (
              <>
                <span>{category.name}</span>
                <div className="category-actions" onClick={(e) => e.stopPropagation()}>
                  <button onClick={() => handleEditStart(category)} title="Edit">✏️</button>
                  <button onClick={() => handleDelete(category.id)} title="Delete">🗑️</button>
                </div>
              </>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default CategorySidebar;
