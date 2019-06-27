﻿using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using System;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Adapters;
using Toggl.Droid.Extensions;
using Toggl.Shared.Extensions;

namespace Toggl.Droid.Fragments
{
    public sealed partial class SelectDateFormatFragment : ReactiveDialogFragment<SelectDateFormatViewModel>
    {
        public SelectDateFormatFragment() { }

        public SelectDateFormatFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.SelectDateFormatFragment, null);

            InitializeViews(view);

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Context));

            var selectDateRecyclerAdapter = new SelectDateFormatRecyclerAdapter();
            selectDateRecyclerAdapter.Items = ViewModel.DateTimeFormats;

            recyclerView.SetAdapter(selectDateRecyclerAdapter);

            selectDateRecyclerAdapter.ItemTapObservable
                .Subscribe(ViewModel.SelectDateFormat.Inputs)
                .DisposedBy(DisposeBag);
        }

        public override void OnResume()
        {
            base.OnResume();

            Dialog.Window.SetDefaultDialogLayout(Activity, Context, heightDp: 400);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            DisposeBag.Dispose();
        }
    }
}
