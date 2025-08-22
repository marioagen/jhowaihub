import { createRouter, createWebHashHistory } from "vue-router";
import DocumentUpload from "@/components/pages/document/upload";
import DocumentList from "@/components/pages/document/list";
import NormalizeIndex from "@/components/pages/normalize/loading";
import AnalyzerIndex from "@/components/pages/analyzer";

import LoginIndex from "@/pages/login.vue";
import LogoutPage from "@/pages/logout";
import UnauthorizedPage from "@/pages/unauthorized.vue";
import TypesPage from "@/pages/types.vue";
import QuestionsPage from "@/pages/questions.vue";
import ManagementPage from "@/pages/management.vue";
import QuizzesPage from "@/pages/quizzes/index.vue";
import NewQuizz from "@/pages/quizzes/newQuizz.vue";
import EditQuizz from "@/pages/quizzes/editQuizz.vue";
import WorkflowPage from "@/pages/workflow/index.vue";
import WorkflowEditor from "@/pages/workflow/editorIndex.vue";
import NewWorkflow from "@/pages/workflow/newWorkflow.vue";
import EditWorkflow from "@/pages/workflow/editWorkflow.vue";

import { hasPermission } from "@/utils/permissions";
function authenticate(to, from, next) {
    const userStr = window.localStorage.getItem("project");
    const user = userStr ? JSON.parse(userStr) : null;
    if (!user) {
        return next({ path: "/" });
    }

    if (user.isLogged !== true) {
        return next({ path: "/" });
    }

    if (!hasPermission(to.meta.module, to.meta.action)) {
        return next({ path: "/unauthorized" });
    }

    return next();
}

const routes = [
    {
        path: "/",
        name: "Login",
        component: LoginIndex,
        meta: { 
            layout: "auth",
        },
    },
    {
        path: "/logout",
        name: "Logout",
        component: LogoutPage,
        meta: {
            public: true
        }
    },
    {
        path: "/unauthorized",
        name: "Unauthorized",
        component: UnauthorizedPage,
        meta: { 
            layout: "auth",
        },
    },
    {
        path: "/document-upload",
        name: "DocumentUpload",
        component: DocumentUpload,
        meta: { 
            layout: "default",
            module: "Documents",
            action: "View",
        },
    },
    {
        path: "/document-list",
        name: "DocumentList",
        component: DocumentList,
        meta: { 
            layout: "default",
            module: "Documents",
            action: "View",
        },
    },
    {
        path: "/types",
        name: "Type",
        component: TypesPage,
        meta: {
            layout: "default",
            module: "Types",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/questions",
        name: "Question",
        component: QuestionsPage,
        meta: { 
            layout: "default",
            module: "Questions",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/quizzes",
        name: "Quiz",
        component: QuizzesPage,
        meta: { 
            layout: "default",
            module: "Quizzes",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/quizzes/new",
        name: "NewQuizz",
        component: NewQuizz,
        meta: { 
            layout: "default",
            module: "Quizzes",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/quizzes/edit/:id",
        name: "EditQuizz",
        component: EditQuizz,
        meta: { 
            layout: "default",
            module: "Quizzes",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/normalize/:id",
        name: "Normalize",
        component: NormalizeIndex,
        meta: { 
            layout: "default",
            module: "Documents",
            action: "View",
        },
    },
    {
        path: "/analyzer/:id",
        name: "Analyzer",
        component: AnalyzerIndex,
        meta: { 
            layout: "default",
            module: "Documents",
            action: "View",
        },
    },
    {
        path: "/management",
        name: "Management",
        component: ManagementPage,
        meta: { 
            layout: "default",
            module: "Management",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/workflow",
        name: "Workflow",
        component: WorkflowPage,
        meta: { 
            layout: "default",
        },
        // beforeEnter: authenticate,
    },
    {
        path: "/workflow/editor",
        name: "WorkflowEditor",
        component: WorkflowEditor,
        meta: { 
            layout: "default",
        },
        // beforeEnter: authenticate,
    },
    {
        path: "/workflow/new",
        name: "NewWorkflow",
        component: NewWorkflow,
        meta: { 
            layout: "default",
        },
        // beforeEnter: authenticate,
    },
    {
        path: "/workflow/edit/:id",
        name: "EditWorkflow",
        component: EditWorkflow,
        meta: { 
            layout: "default",
        },
        // beforeEnter: authenticate,
    },
];

const router = createRouter({
    history: createWebHashHistory(),
    routes,
});

export default router;