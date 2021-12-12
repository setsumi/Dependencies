using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dependencies
{
	public class DataGridSort : DataGrid
	{
		protected void SortDataGrid(object sender, DataGridColumnEventArgs e)
		{
			string category = e.Column.Tag as string;
			if (category == null)
				return;

			SortDirection direction = SortDirection.Descending;

			if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
			{
				direction = SortDirection.Ascending;
				e.Column.SortDirection = DataGridSortDirection.Ascending;
			}
			else
			{
				e.Column.SortDirection = DataGridSortDirection.Descending;
			}

			using (SortedItems.DeferRefresh())
			{
				SortedItems.SortDescriptions.Clear();
				SortedItems.SortDescriptions.Add(new SortDescription(category, direction));
			}

			// Remove sorting indicators from other columns
			foreach (var dgColumn in Columns)
			{
				if (dgColumn.Tag != null && dgColumn.Tag.ToString() != category)
				{
					dgColumn.SortDirection = null;
				}
			}
		}

		protected Toolkit.Uwp.UI.AdvancedCollectionView SortedItems;

	}
}
