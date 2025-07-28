import { createRouter, createWebHashHistory } from "vue-router";
import LoginIndex from "@/components/pages/login";
import LogoutIndex from "@/components/pages/logout";
import DocumentUpload from "@/components/pages/document/upload";
import DocumentList from "@/components/pages/document/list";
import TypeManager from "@/components/pages/manager/type";
import QuestionManager from "@/components/pages/manager/question";
import QuizFormNew from "@/components/pages/quiz/form-new";
import QuizFormEdit from "@/components/pages/quiz/form-edit";
import QuizManager from "@/components/pages/manager/quiz";
import NormalizeIndex from "@/components/pages/normalize/loading";
import AnalyzerIndex from "@/components/pages/analyzer";
import UserIndex from "@/components/pages/user/index";

import TypesPage from "@/pages/types.vue";
import UserManagePage from "@/pages/user-manager.vue";

import { hasPermission } from "@/utils/permissions";

function authenticate(to, from, next) {
    var usuario = JSON.parse(window.localStorage.getItem("project"));
    if (usuario != null) {
        if (usuario.isLogged === true) {
            next();
        }
    } else
        next({
            path: "/",
        });
}

function requirePermission(permissionKey) {
    return (to, from, next) => {
        authenticate(to, from, () => {
            const permissionsList = permissionKey.split(',');
            let isAllowed = false;

            for (const permission of permissionsList) {
                if(hasPermission(permission)) {
                    isAllowed = true;
                    break;
                }
            }

            if (isAllowed) {
                next();
            } else {
                next({ path: '/unauthorized' });
            }
        });
    };
}

const routes = [
    {
        path: "/",
        name: "Login",
        component: LoginIndex,
        meta: { layout: "auth" },
    },
    {
        path: "/logout",
        name: "Logout",
        component: LogoutIndex,
    },
    {
        path: "/document-upload",
        name: "DocumentUpload",
        component: DocumentUpload,
        meta: { layout: "default" },
        beforeEnter: requirePermission("Documents:Upload"),
    },
    {
        path: "/document-list",
        name: "DocumentList",
        component: DocumentList,
        meta: { layout: "default" },
        beforeEnter: requirePermission("Documents:View"),
    },
    {
        path: "/types",
        name: "Type",
        component: TypesPage,
        meta: {
            layout: "default",
        },
        beforeEnter: requirePermission("Types:View"),
    },
    {
        path: "/manage-question",
        name: "Question",
        component: QuestionManager,
        meta: { layout: "default" },
        beforeEnter: requirePermission("Questions:View"),
    },
    {
        path: "/quiz-new",
        name: "QuizNew",
        component: QuizFormNew,
        meta: { layout: "default" },
        beforeEnter: requirePermission("Quiz:Create"),
    },
    {
        path: "/quiz-edit/:id",
        name: "QuizEdit",
        component: QuizFormEdit,
        meta: { layout: "default" },
        beforeEnter: requirePermission("Quiz:Edit"),
    },
    {
        path: "/manage-quiz",
        name: "Quiz",
        component: QuizManager,
        meta: { layout: "default" },
        beforeEnter: requirePermission("Documents:View"),
    },
    {
        path: "/normalize/:id",
        name: "Normalize",
        component: NormalizeIndex,
        meta: { layout: "default" },
        beforeEnter: requirePermission("Documents:View"),
    },
    {
        path: "/analyzer/:id",
        name: "Analyzer",
        component: AnalyzerIndex,
        meta: { layout: "default" },
        beforeEnter: requirePermission("Documents:View"),
    },
    {
        path: "/manage-user",
        name: "UserManage",
        component: UserManagePage,
        meta: { layout: "default" },
        beforeEnter: requirePermission("ManageUsers:View"),
    },
];

const router = createRouter({
    history: createWebHashHistory(process.env.BASE_URL),
    routes,
});

export default router;
