using System;
using Unity.Collections;
using Unity.Jobs;

namespace Unity.Entities
{
    internal enum Command : byte
    {
        CreateEntity,
        Add,
        Remove,
        Set,
        AddDestroyMessage,
    }

    internal struct CommandData<T> where T : struct, IComponentData
    {
        public Entity entity;
        public T data;
        public Command command;
    }

    public struct SampleCommandBuffer<T> : IDisposable where T : struct, IComponentData
    {
        NativeQueue<CommandData<T>> commandBuffer;

        public SampleCommandBuffer(Allocator label)
        {
            commandBuffer = new NativeQueue<CommandData<T>>(label);
        }

        public void Dispose()
        {
            commandBuffer.Dispose();
        }

        public void Playback(EntityManager mgr, JobHandle inputDeps)
        {
            inputDeps.Complete();
            Playback(mgr);
        }

        public void Playback(EntityManager mgr)
        {
            var createEntity = Entity.Null;

            while (commandBuffer.Count > 0)
            {
                var commandData = commandBuffer.Dequeue();

                if (commandData.command == Command.CreateEntity)
                {
                    createEntity = mgr.CreateEntity();
                }
                else
                {
                    var entity = commandData.entity == Entity.Null ? createEntity : commandData.entity;

                    if (commandData.command == Command.Add)
                    {
                        mgr.AddComponentData(entity, commandData.data);
                    }
                    else if (commandData.command == Command.Remove)
                    {
                        mgr.RemoveComponent<T>(entity);
                    }
                    else if (commandData.command == Command.Set)
                    {
                        mgr.SetComponentData(entity, commandData.data);
                    }
                    else if (commandData.command == Command.AddDestroyMessage)
                    {
                        mgr.AddComponent<OnDestroyMessage>(entity);
                    }
                }
            }
        }

        public void Playback(EntityCommandBufferSystem entityCommandBufferSystem, JobHandle inputDeps)
        {
            inputDeps.Complete();
            Playback(entityCommandBufferSystem);
        }

        public void Playback(EntityCommandBufferSystem entityCommandBufferSystem)
        {
            if (commandBuffer.Count <= 0) return;

            var entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

            var createEntity = Entity.Null;

            while (commandBuffer.Count > 0)
            {
                var commandData = commandBuffer.Dequeue();

                if (commandData.command == Command.CreateEntity)
                {
                    createEntity = entityCommandBuffer.CreateEntity();
                }
                else
                {
                    var entity = commandData.entity == Entity.Null ? createEntity : commandData.entity;

                    if (commandData.command == Command.Add)
                    {
                        entityCommandBuffer.AddComponent(entity, commandData.data);
                    }
                    else if (commandData.command == Command.Remove)
                    {
                        entityCommandBuffer.RemoveComponent<T>(entity);
                    }
                    else if (commandData.command == Command.Set)
                    {
                        entityCommandBuffer.SetComponent(entity, commandData.data);
                    }
                    else if (commandData.command == Command.AddDestroyMessage)
                    {
                        entityCommandBuffer.AddComponent<OnDestroyMessage>(entity);
                    }
                }
            }
        }

        public Concurrent ToConcurrent()
        {
            return new Concurrent { commandBuffer = commandBuffer.AsParallelWriter() };
        }

        public struct Concurrent
        {
            internal NativeQueue<CommandData<T>>.ParallelWriter commandBuffer;

            public Entity CreateEntity()
            {
                commandBuffer.Enqueue(new CommandData<T> { command = Command.CreateEntity });
                return Entity.Null;
            }
            public void AddComponent(Entity entity, in T data)
            {
                commandBuffer.Enqueue(new CommandData<T> { command = Command.Add, entity = entity, data = data });
            }
            public void RemoveComponent(Entity entity, in T data)
            {
                commandBuffer.Enqueue(new CommandData<T> { command = Command.Remove, entity = entity });
            }
            public void SetComponent(Entity entity, in T data)
            {
                commandBuffer.Enqueue(new CommandData<T> { command = Command.Set, entity = entity, data = data });
            }
            public void AddDestroyMessage(Entity entity)
            {
                commandBuffer.Enqueue(new CommandData<T> { command = Command.AddDestroyMessage, entity = entity });
            }
        }
    }

    public struct SampleBufferCommandBuffer<T> : IDisposable where T : struct, IBufferElementData
    {
        public enum Command : byte
        {
            AddBuffer,
            AddData,
            RemoveBuffer,
            ClearBuffer,
            //Set,
        }

        public struct CommandData
        {
            public Entity entity;
            public T data;
            public Command command;
        }


        NativeQueue<CommandData> commandBuffer;

        public SampleBufferCommandBuffer(Allocator label)
        {
            commandBuffer = new NativeQueue<CommandData>(label);
        }

        public void Dispose()
        {
            commandBuffer.Dispose();
        }

        public void Playback(EntityManager mgr, JobHandle inputDeps)
        {
            inputDeps.Complete();
            Playback(mgr);
        }

        public void Playback(EntityManager mgr)
        {
            while (commandBuffer.Count > 0)
            {
                var commandData = commandBuffer.Dequeue();

                if (commandData.command == Command.AddBuffer)
                {
                    mgr.AddBuffer<T>(commandData.entity);
                }
                else if (commandData.command == Command.AddData)
                {
                    mgr.GetBuffer<T>(commandData.entity).Add(commandData.data);
                }
                else if (commandData.command == Command.RemoveBuffer)
                {
                    mgr.RemoveComponent<T>(commandData.entity);
                }
                else if (commandData.command == Command.ClearBuffer)
                {
                    mgr.GetBuffer<T>(commandData.entity).Clear();
                }
            }
        }

        public void Playback(EntityCommandBufferSystem entityCommandBufferSystem, JobHandle inputDeps)
        {
            inputDeps.Complete();
            Playback(entityCommandBufferSystem);
        }

        public void Playback(EntityCommandBufferSystem entityCommandBufferSystem)
        {
            if (commandBuffer.Count <= 0) return;

            var entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

            while (commandBuffer.Count > 0)
            {
                var commandData = commandBuffer.Dequeue();

                if (commandData.command == Command.AddBuffer)
                {
                    entityCommandBuffer.AddBuffer<T>(commandData.entity);
                }
                else if (commandData.command == Command.AddData)
                {
                    entityCommandBuffer.AddBuffer<T>(commandData.entity).Add(commandData.data);
                }
                else if (commandData.command == Command.RemoveBuffer)
                {
                    entityCommandBuffer.RemoveComponent<T>(commandData.entity);
                }
                else if (commandData.command == Command.ClearBuffer)
                {
                    entityCommandBuffer.SetBuffer<T>(commandData.entity).Clear();
                }
            }
        }

        public Concurrent ToConcurrent()
        {
            return new Concurrent { commandBuffer = commandBuffer.AsParallelWriter() };
        }

        public struct Concurrent
        {
            internal NativeQueue<CommandData>.ParallelWriter commandBuffer;

            public void AddData(Entity entity, T data)
            {
                commandBuffer.Enqueue(new CommandData { command = Command.AddData, entity = entity, data = data });
            }
            public void AddBuffer(Entity entity)
            {
                commandBuffer.Enqueue(new CommandData { command = Command.AddBuffer, entity = entity });
            }
            public void RemoveBuffer(Entity entity)
            {
                commandBuffer.Enqueue(new CommandData { command = Command.RemoveBuffer, entity = entity });
            }
            public void ClearBuffer(Entity entity)
            {
                commandBuffer.Enqueue(new CommandData { command = Command.ClearBuffer, entity = entity });
            }
        }
    }
}
