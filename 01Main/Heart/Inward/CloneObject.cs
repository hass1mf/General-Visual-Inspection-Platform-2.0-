using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
//CloneObject类自己独立，无需其他引用
//CloneObject类用于对象转换
//被ModuleFormBase.sc调用  m_ModuleObjBaseBack = CloneObject.DeepCopy(ModuleObjBase);  第45行

namespace Heart.Inward
{
    public class CloneObject
    {
        /// <summary>
        /// 对象深拷贝
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>内存地址不同，数据相同的对象</returns>
        public static T DeepCopy<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                //序列化成流
                bf.Serialize(ms, obj);      //深拷贝时，拷贝的对象一定要加上序列化的标识
                ms.Seek(0, SeekOrigin.Begin);
                //反序列化成对象

                retval = bf.Deserialize(ms);
            }
            return (T)retval;

        }
    }
}
