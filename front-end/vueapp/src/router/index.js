import { createRouter, createWebHashHistory } from "vue-router";

import DocumentsUpload from "@/pages/documents/uploads.vue";
import DocumentsPage from "@/pages/documents/index.vue";
import NormalizeIndex from "@/components/documents/EmbeddingDocument";
import AnalyzerIndex from "@/components/pages/analyzer";

import LoginIndex from "@/pages/login.vue";
import LogoutPage from "@/pages/logout";
import UnauthorizedPage from "@/pages/unauthorized.vue";

// import TypesPage from "@/pages/managementQuizzes/types/types.vue";
// import QuestionsPage from "@/pages/managementQuizzes/questions/questions.vue";
import ManagementPage from "@/pages/management/index.vue";
import DashboardPage from "@/pages/dashboard.vue";

import NewUser from "@/pages/management/users/newUser.vue";
import EditUser from "@/pages/management/users/editUser.vue";

import NewTeam from "@/pages/management/teams/newTeam.vue";
import EditTeam from "@/pages/management/teams/editTeam.vue";
import NewProfile from "@/pages/management/profiles/newProfile.vue";
import EditProfile from "@/pages/management/profiles/editProfile.vue";

import ManagementQuizzesPage from "@/pages/managementQuizzes/index.vue";
// import QuizzesPage from "@/pages/managementQuizzes/quizzes/index.vue";
import NewQuizz from "@/pages/managementQuizzes/quizzes/newQuizz.vue";
import EditQuizz from "@/pages/managementQuizzes/quizzes/editQuizz.vue";

import WorkflowPage from "@/pages/workflow/index.vue";
import WorkflowManagement from "@/pages/workflow/management.vue";
import NewWorkflow from "@/pages/workflow/newWorkflow.vue";
import EditWorkflow from "@/pages/workflow/editWorkflow.vue";

import ToolsPage from "@/pages/tools.vue";
import NewFlow from "@/pages/flows/newFlow.vue";
import EditFlow from "@/pages/flows/editFlow.vue";

import PromptPage from "@/pages/prompts/index.vue";
import PromptNew from "@/pages/prompts/newPrompt.vue";
import PromptImport from "@/pages/prompts/import.vue";
import HomePage from "@/pages/home.vue";

import TemplatePage from "@/pages/templates/index.vue";
import TemplateDetail from "@/pages/templates/templateDetail.vue";

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

function authenticateBasic(to, from, next) {
    const userStr = window.localStorage.getItem("project");
    const user = userStr ? JSON.parse(userStr) : null;
    if (!user) {
        return next({ path: "/" });
    }

    if (user.isLogged !== true) {
        return next({ path: "/" });
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
            public: true,
        },
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
        path: "/home",
        name: "Home",
        component: HomePage,
        meta: {
            layout: "default",
        },
        beforeEnter: authenticateBasic,
    },
    {
        path: "/dashboard",
        name: "Dashboard",
        component: DashboardPage,
        meta: {
            layout: "default",
            module: "Dashboard",
            action: "View",
        },
        beforeEnter: authenticate,
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
        beforeEnter: authenticate,
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
        beforeEnter: authenticate,
    },
    // {
    //     path: "/types",
    //     name: "Type",
    //     component: TypesPage,
    //     meta: {
    //         layout: "default",
    //         module: "Types",
    //         action: "View",
    //     },
    //     beforeEnter: authenticate,
    // },
    // {
    //     path: "/questions",
    //     name: "Question",
    //     component: QuestionsPage,
    //     meta: {
    //         layout: "default",
    //         module: "Questions",
    //         action: "View",
    //     },
    //     beforeEnter: authenticate,
    // },
    {
        path: "/management-quizzes",
        name: "ManagementQuizzes",
        component: ManagementQuizzesPage,
        meta: {
            layout: "default",
            module: "Quizzes",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    // {
    //     path: "/quizzes",
    //     name: "Quiz",
    //     component: QuizzesPage,
    //     meta: {
    //         layout: "default",
    //         module: "Quizzes",
    //         action: "View",
    //     },
    //     beforeEnter: authenticate,
    // },
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
        path: "/analyzer/:documentId/:cardId",
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
        path: "/management/users/edit/:email",
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
            module: "Workflow",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/workflow/management/:phase?",
        name: "WorkflowManagement",
        component: WorkflowManagement,
        meta: {
            layout: "default",
            module: "Workflow",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/workflow/new/:phase?/:workflowId?",
        name: "NewWorkflow",
        component: NewWorkflow,
        meta: {
            layout: "default",
            module: "Workflow",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/workflow/edit/:id/:phase?",
        name: "EditWorkflow",
        component: EditWorkflow,
        meta: {
            layout: "default",
            module: "Workflow",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/tools",
        name: "Tools",
        component: ToolsPage,
        meta: {
            layout: "default",
            module: "Tools",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/flow/:stepOrder/:phase/:workflowId/:stepId/:hasStepTools",
        name: "NewFlow",
        component: NewFlow,
        meta: {
            layout: "default",
            module: "Workflow",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/flow/:stepId/:stepOrder/:phase/:workflowId/:hasStepTools",
        name: "EditFlow",
        component: EditFlow,
        meta: {
            layout: "default",
            module: "Workflow",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/prompts",
        name: "Prompt",
        component: PromptPage,
        meta: {
            layout: "default",
            module: "Prompts",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/prompts/import",
        name: "PromptImport",
        component: PromptImport,
        meta: {
            layout: "default",
            module: "Prompts",
            action: "View",
        },
        beforeEnter: authenticate,
    },
    {
        path: "/prompts/new",
        name: "PromptNew",
        component: PromptNew,
        meta: {
            layout: "default",
            module: "Prompts",
            action: "View",
        },
        beforeEnter: authenticate,
        props: true,
    },
    // {
    //     path: "/templates",
    //     name: "Template",
    //     component: TemplatePage,
    //     meta: {
    //         layout: "default",
    //         module: "Templates",
    //         action: "View",
    //     },
    //     beforeEnter: authenticate,
    // },
    // {
    //     path: "/templates/new",
    //     name: "TemplateNew",
    //     component: TemplateDetail,
    //     meta: {
    //         layout: "default",
    //         module: "Templates",
    //         action: "View",
    //     },
    //     beforeEnter: authenticate,
    // },
    // {
    //     path: "/templates/edit/:id",
    //     name: "TemplateEdit",
    //     component: TemplateDetail,
    //     meta: {
    //         layout: "default",
    //         module: "Templates",
    //         action: "View",
    //     },
    //     beforeEnter: authenticate,
    // },
];

const router = createRouter({
    history: createWebHashHistory(),
    routes,
});
export default router;
