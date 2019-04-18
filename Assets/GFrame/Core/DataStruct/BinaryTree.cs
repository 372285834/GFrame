using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructure
{
    /// <summary>
    /// 二叉链表结点类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public  class TreeNode<T>
    {
        private T data;               //数据域
        private TreeNode<T> lChild;   //左孩子   树中一个结点的子树的根结点称为这个结点的孩子
        private TreeNode<T> rChild;   //右孩子

        public TreeNode(T val, TreeNode<T> lp, TreeNode<T> rp)
        {
            data = val;
            lChild = lp;
            rChild = rp;
        }

        public TreeNode(TreeNode<T> lp, TreeNode<T> rp)
        {
            data = default(T);
            lChild = lp;
            rChild = rp;
        }

        public TreeNode(T val)
        {
            data = val;
            lChild = null;
            rChild = null;
        }

        public TreeNode()
        {
            data = default(T);
            lChild = null;
            rChild = null;
        }

        public T Data
        {
            get { return data; }
            set { data = value; }
        }

        public TreeNode<T> LChild
        {
            get { return lChild; }
            set { lChild = value; }
        }

        public TreeNode<T> RChild
        {
            get { return rChild; }
            set { rChild = value; }
        }

    }

    /// <summary>
    /// 定义索引文件结点的数据类型
    /// </summary>
    public struct indexnode
    {
        int key;         //键
        int offset;      //位置
        public indexnode(int key, int offset)
        {
            this.key = key;
            this.offset = offset;
        }

        //键属性
        public int Key
        {
            get { return key; }
            set { key = value; }
        }
        //位置属性
        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        }


    }


    public class LinkBinaryTree<T>
    {
        private TreeNode<T> head;       //头引用

        public TreeNode<T> Head
        {
            get { return head; }
            set { head = value; }
        }

        public LinkBinaryTree()
        {
            head = null;
        }

        public LinkBinaryTree(T val)
        {
            TreeNode<T> p = new TreeNode<T>(val);
            head = p;
        }

        public LinkBinaryTree(T val, TreeNode<T> lp, TreeNode<T> rp)
        {
            TreeNode<T> p = new TreeNode<T>(val, lp, rp);
            head = p;
        }

        //判断是否是空二叉树
        public bool IsEmpty()
        {
            if (head == null)
                return true;
            else
                return false;
        }

        //获取根结点
        public TreeNode<T> Root()
        {
            return head;
        }

        //获取结点的左孩子结点
        public TreeNode<T> GetLChild(TreeNode<T> p)
        {
            return p.LChild;
        }

        public TreeNode<T> GetRChild(TreeNode<T> p)
        {
            return p.RChild;
        }

        //将结点p的左子树插入值为val的新结点，原来的左子树称为新结点的左子树
        public void InsertL(T val, TreeNode<T> p)
        {
            TreeNode<T> tmp = new TreeNode<T>(val);
            tmp.LChild = p.LChild;
            p.LChild = tmp;
        }

        //将结点p的右子树插入值为val的新结点，原来的右子树称为新节点的右子树
        public void InsertR(T val, TreeNode<T> p)
        {
            TreeNode<T> tmp = new TreeNode<T>(val);
            tmp.RChild = p.RChild;
            p.RChild = tmp;
        }

        //若p非空 删除p的左子树
        public TreeNode<T> DeleteL(TreeNode<T> p)
        {
            if ((p == null) || (p.LChild == null))
                return null;
            TreeNode<T> tmp = p.LChild;
            p.LChild = null;
            return tmp;
        }

        //若p非空 删除p的右子树
        public TreeNode<T> DeleteR(TreeNode<T> p)
        {
            if ((p == null) || (p.RChild == null))
                return null;
            TreeNode<T> tmp = p.RChild;
            p.RChild = null;
            return tmp;
        }

        //编写算法 在二叉树中查找值为value的结点

        public TreeNode<T> Search(TreeNode<T> root, T value)
        {
            TreeNode<T> p = root;
            if (p == null)
                return null;
            if (p.Data.Equals(value))
                return p;
            if (p.LChild != null)
            {
                return Search(p.LChild, value);
            }
            if (p.RChild != null)
            {
                return Search(p.RChild, value);
            }
            return null;
        }

        //判断是否是叶子结点
        public bool IsLeaf(TreeNode<T> p)
        {
            if ((p != null) && (p.RChild == null) && (p.LChild == null))
                return true;
            else
                return false;
        }


        //中序遍历
        //遍历根结点的左子树->根结点->遍历根结点的右子树 
        public void inorder(TreeNode<T> ptr)
        {
            if (IsEmpty())
            {
                //Console.WriteLine("Tree is Empty !");
                return;
            }
            if (ptr != null)
            {
                inorder(ptr.LChild);
                //Console.WriteLine(ptr.Data + " ");
                inorder(ptr.RChild);
            }
        }


        //先序遍历
        //根结点->遍历根结点的左子树->遍历根结点的右子树 
        public void preorder(TreeNode<T> ptr)
        {
            if (IsEmpty())
            {
                //Console.WriteLine("Tree is Empty !");
                return;
            }
            if (ptr != null)
            {
                //Console.WriteLine(ptr.Data + " ");
                preorder(ptr.LChild);
                preorder(ptr.RChild);
            }
        }


        //后序遍历
        //遍历根结点的左子树->遍历根结点的右子树->根结点
        public void postorder(TreeNode<T> ptr)
        {
            if (IsEmpty())
            {
                //Console.WriteLine("Tree is Empty !");
                return;
            }
            if (ptr != null)
            {
                postorder(ptr.LChild);
                postorder(ptr.RChild);
                //Console.WriteLine(ptr.Data + "");
            }
        }


        //层次遍历
        //引入队列 
        public void LevelOrder(TreeNode<T> root)
        {
            if (root == null)
            {
                return;
            }
            CSeqQueue<TreeNode<T>> sq = new CSeqQueue<TreeNode<T>>(50);
            sq.EnQueue(root);
            while (!sq.IsEmpty())
            {
                //结点出队
                TreeNode<T> tmp = sq.DeQueue();
                //处理当前结点
                //Console.WriteLine("{0}", tmp);
                //将当前结点的左孩子结点入队
                if (tmp.LChild != null)
                {
                    sq.EnQueue(tmp.LChild);
                }
                if (tmp.RChild != null)
                {
                    sq.EnQueue(tmp.RChild);
                }
            }
        }
    }

    /// <summary>
    /// 二叉搜索树：结点的左子节点的值永远小于该结点的值，而右子结点的值永远大于该结点的值 称为二叉搜索树
    /// </summary>
    public class LinkBinarySearchTree : LinkBinaryTree<indexnode>
    {
        //定义增加结点的方法
        public void insert(indexnode element)
        {
            TreeNode<indexnode> tmp, parent = null, currentNode = null;
            //调用FIND方法
            find(element, ref parent, ref currentNode);
            if (currentNode != null)
            {
                //Console.WriteLine("Duplicates words not allowed");
                return;
            }
            else
            {
                //创建结点
                tmp = new TreeNode<indexnode>(element);
                if (parent == null)
                    Head = tmp;
                else
                {
                    if (element.Key < parent.Data.Key)
                        parent.LChild = tmp;
                    else
                        parent.RChild = tmp;
                }
            }
        }

        //定义父结点
        public void find(indexnode element, ref TreeNode<indexnode> parent, ref TreeNode<indexnode> currentNode)
        {
            currentNode = Head;
            parent = null;
            while ((currentNode != null) && (currentNode.Data.Key.ToString() != element.Key.ToString()) && (currentNode.Data.Offset .ToString() != element.Offset .ToString()))//
            {
                parent = currentNode;
                if (element.Key < currentNode.Data.Key)
                    currentNode = currentNode.LChild;
                else
                    currentNode = currentNode.RChild;
            }
        }

        //定位结点
        public void find(int key)
        {
            TreeNode<indexnode> currentNode = Head;
            while ((currentNode != null) && (currentNode.Data.Key.ToString () != key.ToString ()))
            {
                //Console.WriteLine(currentNode.Data.Offset.ToString());
                if (key < currentNode.Data.Key)
                    currentNode = currentNode.LChild;
                else
                    currentNode = currentNode.RChild;
            }
        }

        //中序遍历
        //遍历根结点的左子树->根结点->遍历根结点的右子树 
        public void inorderS(TreeNode<indexnode> ptr)
        {
            if (IsEmpty())
            {
                //Console.WriteLine("Tree is Empty !");
                return;
            }
            if (ptr != null)
            {
                inorderS(ptr.LChild);
                //Console.WriteLine(ptr.Data.Key  + " ");
                inorderS(ptr.RChild);
            }
        }


        //先序遍历
        //根结点->遍历根结点的左子树->遍历根结点的右子树 
        public void preorderS(TreeNode<indexnode> ptr)
        {
            if (IsEmpty())
            {
                //Console.WriteLine("Tree is Empty !");
                return;
            }
            if (ptr != null)
            {
                //Console.WriteLine(ptr.Data.Key  + " ");
                preorderS(ptr.LChild);
                preorderS(ptr.RChild);
            }
        }


        //后序遍历
        //遍历根结点的左子树->遍历根结点的右子树->根结点
        public void postorderS(TreeNode<indexnode> ptr)
        {
            if (IsEmpty())
            {
                //Console.WriteLine("Tree is Empty !");
                return;
            }
            if (ptr != null)
            {
                postorderS(ptr.LChild);
                postorderS(ptr.RChild);
                //Console.WriteLine(ptr.Data.Key + "");
            }
        }
    }


    /// <summary>
    /// 循环顺序队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class CSeqQueue<T>
    {
        private int maxsize;       //循环顺序队列的容量
        private T[] data;          //数组，用于存储循环顺序队列中的数据元素
        private int front;         //指示最近一个已经离开队列的元素所占有的位置 循环顺序队列的对头
        private int rear;          //指示最近一个进入队列的元素的位置           循环顺序队列的队尾

        public T this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        //容量属性
        public int Maxsize
        {
            get { return maxsize; }
            set { maxsize = value; }
        }

        //对头指示器属性
        public int Front
        {
            get { return front; }
            set { front = value; }
        }

        //队尾指示器属性
        public int Rear
        {
            get { return rear; }
            set { rear = value; }
        }

        public CSeqQueue()
        {

        }

        public CSeqQueue(int size)
        {
            data = new T[size];
            maxsize = size;
            front = rear = -1;
        }

        //判断循环顺序队列是否为满
        public bool IsFull()
        {
            if ((front == -1 && rear == maxsize - 1) || (rear + 1) % maxsize == front)
                return true;
            else
                return false;
        }

        //清空循环顺序列表
        public void Clear()
        {
            front = rear = -1;
        }

        //判断循环顺序队列是否为空
        public bool IsEmpty()
        {
            if (front == rear)
                return true;
            else
                return false;
        }

        //入队操作
        public void EnQueue(T elem)
        {
            if (IsFull())
            {
                //Console.WriteLine("Queue is Full !");
                return;
            }
            rear = (rear + 1) % maxsize;
            data[rear] = elem;
        }

        //出队操作
        public T DeQueue()
        {
            if (IsEmpty())
            {
                //Console.WriteLine("Queue is Empty !");
                return default(T);
            }
            front = (front + 1) % maxsize;
            return data[front];
        }

        //获取对头数据元素
        public T GetFront()
        {
            if (IsEmpty())
            {
                //Console.WriteLine("Queue is Empty !");
                return default(T);
            }
            return data[(front + 1) % maxsize];//front从-1开始
        }

        //求循环顺序队列的长度
        public int GetLength()
        {
            return (rear - front + maxsize) % maxsize;
        }
    }

    /*
class BinaryTree
    {

        static void Main(string[] args)
        {
            LinkBinarySearchTree bs = new LinkBinarySearchTree();
            while (true)
            {
                //菜单
                Console.WriteLine("\nMenu");
                Console.WriteLine("1.创建二叉搜索树");
                Console.WriteLine("2.执行中序遍历");
                Console.WriteLine("3.执行先序遍历");
                Console.WriteLine("4.执行后序遍历");
                Console.WriteLine("5.显示搜索一个结点的路径");
                Console.WriteLine("6.exit");

                Console.WriteLine("\n输入你的选择(1-5)");
                char ch = Convert.ToChar(Console.ReadLine());
                Console.WriteLine();

                switch (ch)
                {
                    case '1':
                        {
                            int key, offset;
                            string flag;
                            do
                            {
                                Console.WriteLine("请输入键：");
                                key = Convert.ToInt32(Console.ReadLine());
                                Console.WriteLine("请输入位置：");
                                offset = Convert.ToInt32(Console.ReadLine());
                                indexnode element = new indexnode(key, offset);
                                bs.insert(element);
                                Console.WriteLine("继续添加新的结点(Y/N)？");
                                flag = Console.ReadLine();
                            } while (flag == "Y" || flag == "y");
                        }
                        break;
                    case '2':
                        {
                            bs.inorderS(bs.Head);
                        }
                        break;
                    case '3':
                        {
                            bs.preorderS(bs.Head);
                        }
                        break;
                    case '4':
                        {
                            bs.postorderS(bs.Head);
                        }
                        break;
                    case '5':
                        {
                            int key;
                            Console.WriteLine("请输入键");
                            key = Convert.ToInt32(Console.ReadLine());
                            bs.find(key);
                        }
                        break;
                    case '6':
                        return;
                    default:
                        {
                            Console.WriteLine("Invalid option");
                            break;
                        }
                }
            }
        }
     * */


    /// <summary>
    /// 哈夫曼树结点类
    /// </summary>
    public class HNode
    {
        private int weight;          //结点权值
        private int lChild;          //左孩子结点
        private int rChild;          //右孩子结点
        private int parent;          //父结点

        public int Weight
        {
            get { return weight; }
            set { weight = value; }
        }

        public int LChild
        {
            get { return lChild; }
            set { lChild = value; }
        }

        public int RChild
        {
            get { return rChild; }
            set { rChild = value; }
        }

        public int Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public HNode()
        {
            weight = 0;
            lChild = -1;
            rChild = -1;
            parent = -1;
        }
    }


    public class HuffmanTree
    {
        private HNode[] data;       //结点数组
        private int leafNum;        //叶子结点数目

        public HNode this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public int LeafNum
        {
            get { return leafNum; }
            set { leafNum = value; }
        }

        public HuffmanTree(int n)
        {
            //由哈夫曼树的构造思想，用一个数组存放原来的n个叶子结点和构造过程中产生
            //临时生成的结点，数组的大小为2n-1
            data = new HNode[2 * n - 1];
            for (int i = 0; i < 2 * n - 1; i++)
            {
                data[i] = new HNode();
            }
            leafNum = n;
        }


        //创建哈夫曼树
        public void Create()
        {
            int m1, m2, x1, x2;
            //输入n个叶子结点的权值
            for (int i = 0; i < this.leafNum; ++i)
            {
                data[i].Weight = Convert.ToInt32(Console.ReadLine());
            }
            //处理n个叶子结点，建立哈夫曼树
            for (int i = 0; i < this.leafNum - 1; ++i)
            {
                m1 = m2 = Int32.MaxValue;
                x1 = x2 = 0;
                for (int j = 0; j < this.leafNum + i; ++j)//2*n-1
                {
                    if ((data[j].Weight < m1) && (data[j].Parent == -1))
                    {
                        m2 = m1;
                        x2 = x1;
                        m1 = data[j].Weight;
                        x1 = j;
                    }
                    else if ((data[j].Weight < m2) && (data[j].Parent == -1))
                    {
                        m2 = data[j].Weight;
                        x2 = j;
                    }
                }
                data[x1].Parent = this.leafNum + i;
                data[x2].Parent = this.leafNum + i;
                data[this.leafNum + i].Weight = data[x1].Weight + data[x2].Weight;
                data[this.leafNum + i].LChild = x1;
                data[this.leafNum + 1].RChild = x2;
            }
        }


    }
    /*
 static void Main(string[] arg)
        {
            HuffmanTree ht;
            Console.WriteLine("请输入叶节点的个数：");
            int leafNum = Convert.ToInt32(Console.ReadLine());
            ht = new HuffmanTree(leafNum);
            ht.Create();
            Console.WriteLine("位置\t权值\t父结点\t左孩子结点\t右孩子结点");
            for (int i = 0; i < 2 * leafNum - 1; i++)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", i, ht[i].Weight, ht[i].Parent, ht[i].LChild, ht[i].RChild);
            }
            Console.ReadLine();
        }
     * */
}