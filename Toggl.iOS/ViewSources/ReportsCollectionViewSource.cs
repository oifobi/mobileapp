﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Foundation;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.iOS.Cells.Reports;
using UIKit;

namespace Toggl.iOS.ViewSources
{
    public enum ReportsCollectionViewCell
    {
        Summary,
        BarChart,
        DonutChart,
        DonutChartLegendItem,
        Error,
        NoData
    }

    public class ReportsCollectionViewSource : UICollectionViewSource
    {
        private readonly UICollectionView collectionView;

        private IImmutableList<IReportElement> elements;

        private const string summaryCellIdentifier = nameof(summaryCellIdentifier);
        private const string barChartCellIdentifier = nameof(barChartCellIdentifier);
        private const string donutChartCellIdentifier = nameof(donutChartCellIdentifier);
        private const string donutChartLegendCellIdentifier = nameof(donutChartLegendCellIdentifier);
        private const string noDataCellIdentifier = nameof(noDataCellIdentifier);
        private const string errorCellIdentifier = nameof(errorCellIdentifier);
        private const string workspaceCellIdentifier = nameof(workspaceCellIdentifier);

        public ReportsCollectionViewSource(UICollectionView collectionView)
        {
            this.collectionView = collectionView;

            collectionView.RegisterNibForCell(ReportsSummaryCollectionViewCell.Nib, summaryCellIdentifier);
            collectionView.RegisterNibForCell(ReportsBarChartCollectionViewCell.Nib, barChartCellIdentifier);
            collectionView.RegisterNibForCell(ReportsDonutChartCollectionViewCell.Nib, donutChartCellIdentifier);
            collectionView.RegisterNibForCell(ReportsDonutChartLegendCollectionViewCell.Nib, donutChartLegendCellIdentifier);
            collectionView.RegisterNibForCell(ReportsNoDataCollectionViewCell.Nib, noDataCellIdentifier);
            collectionView.RegisterNibForCell(ReportsErrorCollectionViewCell.Nib, errorCellIdentifier);
        }

        public void SetNewElements(IImmutableList<IReportElement> elements)
        {
            this.elements = elements.Where(e => e.GetType().Name != nameof(ReportWorkspaceNameElement)).ToImmutableList();
            collectionView.ReloadData();
            collectionView.CollectionViewLayout.InvalidateLayout();
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            switch (elements[(int)indexPath.Item])
            {
                case ReportSummaryElement _:
                    var summaryCell = collectionView.DequeueReusableCell(summaryCellIdentifier, indexPath) as ReportsSummaryCollectionViewCell;
                    // TODO: populate cell
                    return summaryCell;
                case ReportBarChartElement _:
                    var barChartCell = collectionView.DequeueReusableCell(barChartCellIdentifier, indexPath) as ReportsBarChartCollectionViewCell;
                    // TODO: populate cell
                    barChartCell.BackgroundColor = UIColor.SystemGreenColor;
                    return barChartCell;
                case ReportDonutChartDonutElement _:
                    var donutCell = collectionView.DequeueReusableCell(donutChartCellIdentifier, indexPath) as ReportsDonutChartCollectionViewCell;
                    // TODO: populate cell
                    donutCell.BackgroundColor = UIColor.SystemRedColor;
                    return donutCell;
                case ReportDonutChartLegendItemElement _:
                    var donutLegendItemCell = collectionView.DequeueReusableCell(donutChartLegendCellIdentifier, indexPath) as ReportsDonutChartLegendCollectionViewCell;
                    // TODO: populate cell
                    donutLegendItemCell.BackgroundColor = UIColor.SystemYellowColor;
                    return donutLegendItemCell;
                case ReportNoDataElement _:
                    var noDataCell = collectionView.DequeueReusableCell(noDataCellIdentifier, indexPath) as ReportsNoDataCollectionViewCell;
                    // TODO: populate cell
                    noDataCell.BackgroundColor = UIColor.SystemPurpleColor;
                    return noDataCell;
                default:
                    var errorCell = collectionView.DequeueReusableCell(errorCellIdentifier, indexPath) as ReportsErrorCollectionViewCell;
                    // TODO: populate cell
                    errorCell.BackgroundColor = UIColor.SystemTealColor;
                    return errorCell;
            }
        }

        public override nint NumberOfSections(UICollectionView collectionView)
            => 1;

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
            => elements == null ? 0 : elements.Count();

        public ReportsCollectionViewCell CellTypeAt(NSIndexPath indexPath)
        {
            switch (elements[(int)indexPath.Item])
            {
                case ReportSummaryElement _:
                    return ReportsCollectionViewCell.Summary;
                case ReportBarChartElement _:
                    return ReportsCollectionViewCell.BarChart;
                case ReportDonutChartDonutElement _:
                    return ReportsCollectionViewCell.DonutChart;
                case ReportDonutChartLegendItemElement _:
                    return ReportsCollectionViewCell.DonutChartLegendItem;
                case ReportNoDataElement _:
                    return ReportsCollectionViewCell.NoData;
                default:
                    return ReportsCollectionViewCell.Error;
            }
        }

        public int NumberOfDonutChartLegendItems()
        {
            var num = elements == null
                ? 0
                : elements.Where(e => e is ReportDonutChartLegendItemElement _).Count();
            return num;
        }

        public bool HasDataToDisplay()
            => elements != null && elements.Count > 0;
    }
}