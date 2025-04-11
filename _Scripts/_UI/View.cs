using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Containers;
using UnityEngine;



namespace ProjectM
{
    public abstract class View : MonoBehaviour
    {
        public abstract ViewName ID { get; set; }
        UIView uiView;

        protected string ID_category;
        protected string ID_Name;
        private void Awake()
        {
            uiView = GetComponent<UIView>();
            var _ID = ID.ToString().Split('_');

            uiView.Id.Category = ID_category = _ID[0];
            uiView.Id.Name = ID_Name = _ID[1];

            Init();
        }
        
        protected abstract void Init();

        public void Hide()
        {
            uiView.Hide();
        }
    }
}