#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using PotionCraft.Core.Physics.Authoring;
using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.Physics.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class NodeWrapper<U> : NodeWrapper<object, U>
{
	public NodeWrapper(ICreator<object, U> data) : base(data)
	{
	}
}

public class NodeWrapper<T, U>
{
	public ICreator<T, U> Data { get; }

	private readonly List<IProcessor<U>> children = new();

	public NodeWrapper(ICreator<T, U> data)
	{
		Data = data;
	}

	public void AddChild<V>(NodeWrapper<U, V> child)
	{
		children.Add(new NodeProcessor<U, V>(child));
	}

	public U Process(T parentResult)
	{
		var result = Data.Create(parentResult);

		foreach (var child in children)
			child.Process(result);

		return result;
	}
}

public interface IProcessor<T>
{
	void Process(T parent);
}

public class NodeProcessor<T, U> : IProcessor<T>
{
	private readonly NodeWrapper<T, U> _node;

	public NodeProcessor(NodeWrapper<T, U> node) => _node = node;

	public void Process(T parent) => _node.Process(parent);
}

public interface ICreator<T> : ICreator<object, T>
{
	T ICreator<object, T>.Create(object U)
	{
		return Create();
	}

	T Create();
}

public interface ICreator<T, U>
{
	U Create(T U);
}

public static class CreatorExtensions
{
	public static NodeWrapper<T, U> ToNode<T, U>(this ICreator<T, U> creator)
	{
		return new NodeWrapper<T, U>(creator);
	}
}

public interface IChildAdder<T, U>
{
	void AddChildTo(NodeWrapper<T, U> parent);
}

public class WorldCreator : ICreator<PhysicsWorld>
{
	private readonly PhysicsWorldSetupAuthoring physicsWorldAuthoring;


	public PhysicsWorld Create() => PhysicsWorld.Create(PhysicsWorldDefinition.defaultDefinition);
}

public class BodyCreator : ICreator<PhysicsWorld, PhysicsBody>
{
	private readonly PhysicsBodyAuthoring physicsBodyAuthoring;


	public BodyCreator(PhysicsBodyAuthoring physicsBodyAuthoring)
	{
		this.physicsBodyAuthoring = physicsBodyAuthoring;
	}


	public PhysicsBody Create(PhysicsWorld world) => world.CreateBody(physicsBodyAuthoring.ToBody());
}

public class CircleCreator : ICreator<PhysicsBody, PhysicsShape>, IChildAdder<PhysicsWorld, PhysicsBody>
{
	private readonly PhysicsCircleSetupAuthoring circleAuthoring;

	private readonly PhysicsShapeDefinition blueColor = new() { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.red } };


	public CircleCreator(PhysicsCircleSetupAuthoring circleAuthoring)
	{
		this.circleAuthoring = circleAuthoring;
	}


	public PhysicsShape Create(PhysicsBody body) => body.CreateShape(circleAuthoring.ToCircleGeometry(body.ToPhysicsTransform()), blueColor);


	public void AddChildTo(NodeWrapper<PhysicsWorld, PhysicsBody> parent)
	{
		var circleNode = new NodeWrapper<PhysicsBody, PhysicsShape>(this);
		parent.AddChild(circleNode);
	}
}

public class BoxCreator : ICreator<PhysicsBody, PhysicsShape>, IChildAdder<PhysicsWorld, PhysicsBody>
{
	private readonly PhysicsBoxSetupAuthoring boxAuthoring;


	public BoxCreator(PhysicsBoxSetupAuthoring boxAuthoring)
	{
		this.boxAuthoring = boxAuthoring;
	}


	public PhysicsShape Create(PhysicsBody body) 
		=> body.CreateShape(boxAuthoring.ToGeometry(body.ToPhysicsTransform()), boxAuthoring.ShapeDefinition.WithColor(Color.softRed));

	
	public void AddChildTo(NodeWrapper<PhysicsWorld, PhysicsBody> parent)
	{
		var circleNode = new NodeWrapper<PhysicsBody, PhysicsShape>(this);
		parent.AddChild(circleNode);
	}
}

public class PolygonCreator : ICreator<PhysicsBody, PhysicsShape>, IChildAdder<PhysicsWorld, PhysicsBody>
{
	private readonly PhysicsPolygonSetupAuthoring polygonAuthoring;


	public PolygonCreator(PhysicsPolygonSetupAuthoring boxAuthoring)
	{
		this.polygonAuthoring = boxAuthoring;
	}


	public PhysicsShape Create(PhysicsBody body) 
	{
		using var geometries = polygonAuthoring.ToGeometry(body.ToPhysicsTransform());
		return body.CreateShapeBatch(geometries, polygonAuthoring.ShapeDefinition.WithColor(Color.softRed))[0];
	}
	public void AddChildTo(NodeWrapper<PhysicsWorld, PhysicsBody> parent)
	{
		var circleNode = new NodeWrapper<PhysicsBody, PhysicsShape>(this);
		parent.AddChild(circleNode);
	}
}

[InitializeOnLoad]
public static class PhysicsWorldCreator
{
	private static PhysicsWorld physicsWorld;
	
	private static NodeWrapper<PhysicsWorld> root;
	
	
	static PhysicsWorldCreator()
	{
		EditorApplication.update -= Execute;
		EditorApplication.update += Execute;
	}


	public static void Execute()
	{
		if (physicsWorld.isValid)
			physicsWorld.Destroy();

		if (Selection.gameObjects.Count() == 0)
			return;

		var roots = Selection.gameObjects.Select(t => t.transform.root.gameObject).Distinct();
		root = new NodeWrapper<PhysicsWorld>(new WorldCreator { });
		BuildChildrenRecursive(roots.FirstOrDefault(), root);

		physicsWorld = root.Process(null);
	}


	private static void BuildChildrenRecursive(
		GameObject current,
		object parentNode)
	{
		var drawable = current.GetComponent<IDrawableShape>();
		switch (drawable)
		{
			case PhysicsBodyAuthoring physicsBodyAuthoring:
				var newBody = new BodyCreator(physicsBodyAuthoring).ToNode();
				root.AddChild(newBody);
				parentNode = newBody;
				break;
			case IDrawableShape when parentNode is NodeWrapper<PhysicsWorld, PhysicsBody> physicsBody:
				var adder = GetShapeAdderForBody(drawable);
				adder.AddChildTo(physicsBody);
				break;
		}

		foreach (Transform child in current.transform)
		{
			BuildChildrenRecursive(child.gameObject, parentNode);
		}
	}

	private static IChildAdder<PhysicsWorld, PhysicsBody> GetShapeAdderForBody(IDrawableShape input)
	{
		return input switch
		{
			PhysicsCircleSetupAuthoring circle => new CircleCreator(circle),
			PhysicsBoxSetupAuthoring box => new BoxCreator(box),
			PhysicsPolygonSetupAuthoring polygon => new PolygonCreator(polygon),
			_ => null
		};
	}
}
#endif