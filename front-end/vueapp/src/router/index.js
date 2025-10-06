import { createRouter, createWebHashHistory } from "vue-router";
import DocumentsUpload from "@/pages/documents/uploads.vue";
import DocumentsPage from "@/pages/documents/index.vue";
import NormalizeIndex from "@/components/pages/normalize/loading";
import AnalyzerIndex from "@/components/pages/analyzer";

import LoginIndex from "@/pages/login.vue";
import LogoutPage from "@/pages/logout";
import UnauthorizedPage from "@/pages/unauthorized.vue";
import TypesPage from "@/pages/types.vue";
import QuestionsPage from "@/pages/questions.vue";
import ManagementPage from "@/pages/management.vue";
import ManagementPage from "@/pages/management/index.vue";
import NewUser from "@/pages/management/users/newUser.vue";
import EditUser from "@/pages/management/users/editUser.vue";
import NewTeam from "@/pages/management/teams/newTeam.vue";
import EditTeam from "@/pages/management/teams/editTeam.vue";
import NewProfile from "@/pages/management/profiles/newProfile.vue";
import EditProfile from "@/pages/management/profiles/editProfile.vue";
import QuizzesPage from "@/pages/quizzes/index.vue";
import NewQuizz from "@/pages/quizzes/newQuizz.vue";
import EditQuizz from "@/pages/quizzes/editQuizz.vue";
import WorkflowPage from "@/pages/workflow/index.vue";
import WorkflowManagement from "@/pages/workflow/management.vue";
import NewWorkflow from "@/pages/workflow/newWorkflow.vue";
import EditWorkflow from "@/pages/workflow/editWorkflow.vue";
import ToolsPage from "@/pages/tools.vue";
import NewFlow from "@/pages/flows/newFlow.vue";
import EditFlow from "@/pages/flows/editFlow.vue";

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
        path: "/documents",
        name: "Documents",
        component: DocumentsPage,
        meta: { 
            layout: "default",
            module: "Documents",
            action: "View",
        },
    },
    {
        path: "/documents/upload",
        name: "DocumentsUpload",
        component: DocumentsUpload,
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
        path: "/management/users/new",
        name: "NewUser",
        component: NewUser,
        meta: { 
            layout: "default",
            module: "Management",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/management/users/edit/:id",
        name: "EditUser",
        component: EditUser,
        meta: { 
            layout: "default",
            module: "Management",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/management/teams/new",
        name: "NewTeam",
        component: NewTeam,
        meta: { 
            layout: "default",
            module: "Management",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/management/teams/edit/:id",
        name: "EditTeam",
        component: EditTeam,
        meta: { 
            layout: "default",
            module: "Management",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/management/profiles/new",
        name: "NewProfile",
        component: NewProfile,
        meta: { 
            layout: "default",
            module: "Management",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/management/profiles/edit/:id",
        name: "EditProfile",
        component: EditProfile,
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
        beforeEnter: authenticate,
    },
    {
        path: "/workflow/management",
        name: "WorkflowManagement",
        component: WorkflowManagement,
        meta: { 
            layout: "default",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/workflow/new",
        name: "NewWorkflow",
        component: NewWorkflow,
        meta: { 
            layout: "default",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/workflow/edit/:id",
        name: "EditWorkflow",
        component: EditWorkflow,
        meta: { 
            layout: "default",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/tools",
        name: "Tools",
        component: ToolsPage,
        meta: { 
            layout: "default",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/flow/:stepOrder",
        name: "NewFlow",
        component: NewFlow,
        meta: { 
            layout: "default",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/flow/:id/:stepId/:stepOrder",
        name: "EditFlow",
        component: EditFlow,
        meta: { 
            layout: "default",
        },
        beforeEnter: authenticate,
    },
];

const router = createRouter({
    history: createWebHashHistory(),
    routes,
});
export default router;