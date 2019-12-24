using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace RN.Network
{
    //todo...
    //场景里物体的同步和发送命令的系统
    //物体都在一个节点下面
    //物件都是在场景制作时加入的
    //物体在server和client都必须同时存在
    //物体不可以在运行时创建或删除 只能显示或隐藏
    //通过Transform.GetSiblingIndex()找出对应的物体
    //一个类型的物体一个SceneObjectsSyncSystem

    //命令先发送到服务器 服务器的物件执行完后再广播到客户端
    //根据物件的逻辑 把物件的状态或命令广播到客户端


    [DisableAutoCreation]
    public class SceneObjectsSyncServerSystem<T> : ComponentSystem
    {
        public string objectRootName;
        Transform objectRoot;
        protected void OnInit(Transform root)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }
    }

    [DisableAutoCreation]
    public class SceneObjectsSyncClientSystem<T> : ComponentSystem
    {
        public string objectRootName;
        Transform objectRoot;
        protected void OnInit(Transform root)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }


        //
        void testA()
        {
            var length = 0;
            string LengthA()
            {
                return $"length is {length}";
            }

            string LengthB() => $"length is {length}";

            LengthA();
            LengthB();
        }
        public static double testB(object shape)
        {
            switch (shape)
            {
                case int s when s > 0 || s < -10:
                    return s * s;
                case float c:
                    return c * c;
                case double r:
                    return r * r;
                case var r when r.GetHashCode() == 0:
                    return -2;
                case null:
                    return -1;
                default:
                    return 0;
            }
        }
    }
}
