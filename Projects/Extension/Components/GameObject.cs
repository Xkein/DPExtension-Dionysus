using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Components
{
    public interface IGameObject
    {
        void StartCoroutine(IEnumerator coroutine);
        void StopCoroutine(IEnumerator coroutine);

        Component GetComponent(Predicate<Component> predicate);
        Component GetComponent(int id);
        Component GetComponent(Type type);
        TComponent GetComponent<TComponent>() where TComponent : Component;

        /// <summary>
        /// get components that match predicate in direct children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Component[] GetComponents(Func<Component, bool> predicate);
        Component[] GetComponents();
        Component[] GetComponents(Type type);
        TComponent[] GetComponents<TComponent>() where TComponent : Component;

        
        /// <summary>
        /// get component that match predicate in all children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Component GetComponentInChildren(Predicate<Component> predicate);
        Component GetComponentInChildren(int id);
        Component GetComponentInChildren(Type type);
        TComponent GetComponentInChildren<TComponent>() where TComponent : Component;

        /// <summary>
        /// get components that match predicate in all children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Component[] GetComponentsInChildren(Func<Component, bool> predicate);
        Component[] GetComponentsInChildren();
        Component[] GetComponentsInChildren(Type type);
        TComponent[] GetComponentsInChildren<TComponent>() where TComponent : Component;


        /// <summary>
        /// get component that match predicate in direct children or parents
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Component GetComponentInParent(Predicate<Component> predicate);
        Component GetComponentInParent(int id);
        Component GetComponentInParent(Type type);
        TComponent GetComponentInParent<TComponent>() where TComponent : Component;

        /// <summary>
        ///  get components that match predicate in direct children or parents
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Component[] GetComponentsInParent(Func<Component, bool> predicate);
        Component[] GetComponentsInParent();
        Component[] GetComponentsInParent(Type type);
        TComponent[] GetComponentsInParent<TComponent>() where TComponent : Component;
        
        void AddComponent(Component component);
        /// <summary>
        /// attach component to the child of GameObject
        /// </summary>
        /// <param name="component"></param>
        /// <param name="attached">component to be attached</param>
        void AddComponentEx(Component component, Component attached);
        void RemoveComponent(Component component);
    }
}
