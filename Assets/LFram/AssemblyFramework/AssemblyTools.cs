using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 封装的反射工具
/// </summary>
public class AssemblyTools
{

	/* ------------------------------------------------------反射--------------------------------------------------------- */

	/// <summary>
	/// 反射获取一个值
	/// </summary>
	/// <param name="obj">要获取的目标类</param>
	/// <param name="menberName">要获取的对象(字段、属性、或者方法)</param>
	/// <param name="flags">BindingFlags 类型</param>
	/// <returns></returns>
	public static object GetMemberValue(object obj, string menberName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
	{
		Type type = obj.GetType();
        MemberInfo[] menber = type.GetMember(menberName, flags);
		while (menber == null || menber.Length == 0)
		{
			type = type.BaseType;
			if(type == null)break;

			menber = type.GetMember(menberName, flags);
		}

		object value = null;

		switch (menber[0].MemberType)
		{
			case MemberTypes.Field:
				value = type.GetField(menberName, flags).GetValue(obj);
				break;
			case MemberTypes.Property:
				value = type.GetProperty(menberName, flags).GetValue(obj);
				break;
			default:break;
		}

		return value;
	}



	/// <summary>
	/// 通过反射设置变量数值
	/// </summary>
	/// <param name="info">属性对象</param>
	/// <param name="var">要写入的类对象</param>
	/// <param name="value">要写入的值</param>
	/// <param name="type">要写入的类型</param>
	public static void SetPropertyValue(PropertyInfo info, object var, string value, string type)
	{
		object obj = (object)value;
		if(type == "int")
		{
			obj = System.Convert.ToInt32(obj);
		}
		else if(type == "long")
		{
			obj = System.Convert.ToInt64(obj);
		}
		else if(type == "bool")
		{
			obj = System.Convert.ToBoolean(obj);
		}
		else if(type == "float")
		{
			obj = System.Convert.ToSingle(obj);
		}
		else if(type == "enum")
		{
			//枚举赋值
			obj = TypeDescriptor.GetConverter(info.PropertyType).ConvertFromInvariantString(obj.ToString());
		}		

		info.SetValue(var, obj);
	}


	/* -----------------------------------------------通过反射创建实例--------------------------------------------------------- */

	/// <summary>
	/// 通过反射创建类的实例
	/// </summary>
	/// <param name="name">传入的类名</param>
	/// <returns></returns>
	public static object CreateClass(string name)
	{
		object obj = null;
		Type type = null;

		//遍历程序集
		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
		{
			Type temp = asm.GetType(name);
			if(temp != null)
			{
				type = temp;
				break;
			}
		}

		if(type != null)
		{
			obj = Activator.CreateInstance(type);
		}
		return obj;
	}


	/// <summary>
	/// 通过反射创建泛型List<object>的实例
	/// </summary>
	/// <param name="type">传入List的泛型类型</param>
	/// <returns></returns>
	public static object CreateList(Type type)
	{
		Type listType = typeof(List<>);//获取一个泛型List
		Type specType = listType.MakeGenericType(new System.Type[]{ type });//构造泛型类型为string的List
		//创建实例 new出来
		return Activator.CreateInstance(specType, new object[]{});
	}
}
