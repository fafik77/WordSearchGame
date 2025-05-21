using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Assets.UI_Toolkit.Scripts
{
	//public interface ICategoryOrGroup
	//{
	//	public string Name { get; set; }
	//	public bool IsGroup { get; set; }
	//}
	//   public class Category : ICategoryOrGroup
	//   {
	//       public string Name => throw new NotImplementedException();

	//       public bool IsGroup => throw new NotImplementedException();
	//       public Category(string name, bool hasGroup)
	//       {
	//           this.Name = name;
	//           this.IsGroup = hasGroup;
	//       }
	//   }

	public interface ICategoryOrGroup
	{
		string Name { get; }
		bool HasSubContent { get; }
		IList<TreeViewItemData<ICategoryOrGroup>> GetRoots(ref int id);
	}
	public class CategoryOnly : ICategoryOrGroup
	{
		public string name;
		public List<string> words;
		public string Name => name;
		public bool HasSubContent => false;

		public IList<TreeViewItemData<ICategoryOrGroup>> GetRoots(ref int id) => null;

		public CategoryOnly(string name)
		{
			this.name = name;
			words = new List<string>();
		}
		public CategoryOnly(string name, List<string> words)
		{
			this.name = name;
			this.words = words;
		}
	}

	public class CategoryOrGroup : ICategoryOrGroup
	{
		string name;
		public List<ICategoryOrGroup> SubCategories { get; set; }
		public List<CategoryOnly> Categories { get; set; }
		public List<CategoryOnly> AllCategories { get; set; }
		public string Name => name;
		public bool HasSubContent => (SubCategories != null && SubCategories.Count != 0);
		public bool HasAnyContent => (HasSubContent || (Categories != null && Categories.Count != 0));

		public IList<TreeViewItemData<ICategoryOrGroup>> GetRoots(ref int id)
		{
			return GetRoots(ref id, search: string.Empty);
			//int children = 0;
			//if (SubCategories != null) children += SubCategories.Count;
			//if (Categories != null) children += Categories.Count;
			//var roots = new List<TreeViewItemData<ICategoryOrGroup>>(children);
			//if (SubCategories != null)
			//{
			//	foreach (var item in SubCategories)
			//	{
			//		roots.Add(new TreeViewItemData<ICategoryOrGroup>(id++, item, item.GetRoots(ref id).ToList()));
			//	}
			//}
			//if (Categories != null)
			//{
			//	foreach (var item in Categories)
			//	{
			//		roots.Add(new TreeViewItemData<ICategoryOrGroup>(id++, item));
			//	}
			//}
			//return roots;
		}
		public IList<TreeViewItemData<ICategoryOrGroup>> GetRoots(ref int id, string search)
		{
			int children = 0;
			if (SubCategories != null) children += SubCategories.Count;
			if (Categories != null) children += Categories.Count;
			var roots = new List<TreeViewItemData<ICategoryOrGroup>>(children);
			if (SubCategories != null)
			{
				foreach (var item in SubCategories)
				{
					bool includeDir = (item.Name.ToLower().Contains(search));
					var conteined = new TreeViewItemData<ICategoryOrGroup>(id++, item, ((CategoryOrGroup)item).GetRoots(ref id, includeDir ? string.Empty : search).ToList());
					if (string.IsNullOrEmpty(search) || conteined.hasChildren)
						roots.Add(conteined);
				}
			}
			if (Categories != null)
			{
				foreach (var item in Categories)
				{
					if (string.IsNullOrEmpty(search) || item.Name.ToLower().Contains(search))
						roots.Add(new TreeViewItemData<ICategoryOrGroup>(id++, item));
				}
			}
			return roots;
		}

		public CategoryOrGroup(string name, List<ICategoryOrGroup> subCategories, List<CategoryOnly> categories)
		{
			this.name = name;
			SubCategories = subCategories;
			Categories = categories;
		}
		public CategoryOrGroup(string name, List<ICategoryOrGroup> subCategories)
		{
			this.name = name;
			SubCategories = subCategories;
			Categories = null;
		}
		public CategoryOrGroup(string name, List<CategoryOnly> categories)
		{
			this.name = name;
			SubCategories = null;
			Categories = categories;
		}
		public CategoryOrGroup(string name)
		{
			this.name = name;
			SubCategories = null;
			Categories = null;
		}
		public CategoryOrGroup()
		{
			this.name = string.Empty;
			SubCategories = null;
			Categories = null;
		}
	}


}
