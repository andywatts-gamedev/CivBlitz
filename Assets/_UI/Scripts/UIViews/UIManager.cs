using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkitDemo
{
    [RequireComponent(typeof(UIDocument))]
    public class UIManager : MonoBehaviour
    {

        UIDocument m_MainMenuDocument;

        UIView m_CurrentView;
        UIView m_PreviousView;

        List<UIView> m_AllViews = new List<UIView>();

        UIView m_MenuView;

        const string k_MenuViewName = "MenuScreen";
        
        void OnEnable()
        {
            m_MainMenuDocument = GetComponent<UIDocument>();
 
            SetupViews();
            
            SubscribeToEvents();
      
            ShowModalView(m_MenuView);
        }

        void SubscribeToEvents()
        {
        }

        void OnDisable()
        {
            UnsubscribeFromEvents();

            foreach (UIView view in m_AllViews)
            {
                view.Dispose();
            }
        }

        void UnsubscribeFromEvents()
        {
        }
        
        void Start()
        {
            Time.timeScale = 1f;
        }

        void SetupViews()
        {
            VisualElement root = m_MainMenuDocument.rootVisualElement;

            // Create full-screen modal views: MenuView, CharView, InfoView, ShopView, MailView
            m_MenuView = new MenuView(root.Q<VisualElement>(k_MenuViewName)); // Landing modal screen
            

            // Track modal UI Views in a List for disposal 
            m_AllViews.Add(m_MenuView);

            // UI Views enabled by default
            m_MenuView.Show();
        }


        // Toggle modal screens on/off
        void ShowModalView(UIView newView)
        {
            if (m_CurrentView != null)
                m_CurrentView.Hide();

            m_PreviousView = m_CurrentView;
            m_CurrentView = newView;

            // Show the screen and notify any listeners that the main menu has updated

            if (m_CurrentView != null)
            {
                m_CurrentView.Show();
                // MenuEvents.CurrentViewChanged?.Invoke(m_CurrentView.GetType().Name);
            }
        }

    }
}