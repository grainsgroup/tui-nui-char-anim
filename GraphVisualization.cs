using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;
using System.Windows.Forms; 


namespace Microsoft.Samples.Kinect.SkeletonBasics
{

    class GraphVisualization
    {
        public static void ShowGraph(BidirectionalGraph <Bone, Edge<Bone>> quickGraph, string graphName)
        {           

            //create a form 
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = graphName;
            //create a viewer object 
            Microsoft.Msagl.GraphViewerGdi.GViewer viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            //create a graph object 
            Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("graph");
            //create the graph content 
            
            foreach (var v in quickGraph.Vertices)
            {
                graph.AddNode(v.name);
                graph.FindNode(v.name).Attr.FillColor = Microsoft.Msagl.Drawing.Color.White;
            }
                
            foreach (var e in quickGraph.Edges)
                graph.AddEdge(e.Source.name, e.Target.name);                
            
            
            
/*            
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("A", "C");

            graph.FindNode("A").Attr.FillColor = Microsoft.Msagl.Drawing.Color.White;
            graph.FindNode("B").Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;

            Microsoft.Msagl.Drawing.Node c = graph.FindNode("C");
            c.Attr.FillColor = Microsoft.Msagl.Drawing.Color.PaleGreen;
            c.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Diamond;
 */
            //bind the graph to the viewer 
            viewer.Graph = graph;
            //associate the viewer with the form 
            form.SuspendLayout();
            viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            form.Controls.Add(viewer);
            form.ResumeLayout();
            //show the form 
            form.ShowDialog();
        }
    }
}


